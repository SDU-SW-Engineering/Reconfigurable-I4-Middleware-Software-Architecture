using I4ToolchainDotnetCore.Communication.Kafka;
using I4ToolchainDotnetCore.Logging;
using I4ToolchainDotnetCore.ServiceLayer.Exceptions;
using Orchestrator.Adapter.Interfaces;
using Orchestrator.Adapter.RecipeInterpretation;
using Orchestrator.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Docker.DotNet.Models;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using Orchestrator.DomainObjects;

namespace Orchestrator.Adapter
{
    public class Coordinator : ICoordinator
    {
        private readonly II4Logger log;
        private readonly IConfiguration _conf;
        private static ICoordinator coordinator;
        private ICookbook cookbook;
        private IKafkaProducer _producer;
        private readonly IChefFactory _chefFactory;
        private List<IChef> chefs;
        private Queue<Order> orderQueue;
        private Order currentOrder;
        private readonly string _originId;
        private Order orderToBeStarted;
        private bool _without_configurator;
        public Coordinator(II4Logger log, IConfiguration conf, IKafkaProducer producer, IChefFactory chefFactory)
        {
            _producer = producer;
            _chefFactory = chefFactory;
            _originId = conf.GetValue<string>("AAS_ORIGIN_ID") ?? "i4.sdu.dk/Middleware/Orchestrator";
            _without_configurator = conf.GetValue<bool>("WITHOUT_CONFIGURATOR");

            this.log = log;
            _conf = conf;
            cookbook = new Cookbook(log);
            orderQueue = new Queue<Order>();
            InitializeChefs(conf.GetValue<int>("PARALLEL_PRODUCTION_LIMIT"));
            PopulateCookbook();
            CollectNotifiers();
        }


        private List<INotifier> CollectNotifiers()
        {
            List<INotifier> notifiers = new List<INotifier>();
            notifiers.Add(new SimulationNotifier(log));
            return notifiers;
        }

        private void InitializeChefs(int amount)
        {
            chefs = new List<IChef>();
            for (int i = 0; i < amount; i++)
            {
                var chef = _chefFactory.GetNewChef($"Chef{i}");
                chef.Subscribe(this);
                chefs.Add(chef);
            }
        }

        private void PopulateCookbook()
        {
            try
            {
                Recipe droneRecipe = RecipeInterpreter.ConvertFileToRecipe(log, "Recipes/DroneRecipe.json");
                Recipe resetRecipe = RecipeInterpreter.ConvertFileToRecipe(log, "Recipes/ResetAssets.json");
                Recipe droneWithoutEffimatRecipe =
                    RecipeInterpreter.ConvertFileToRecipe(log, "Recipes/DroneRecipeWithoutEffimat.json");
                Recipe URRecipe = RecipeInterpreter.ConvertFileToRecipe(log, "Recipes/URRecipe.json");
                Recipe ACOPOStrakRecipe = RecipeInterpreter.ConvertFileToRecipe(log, "Recipes/TrackRecipe.json");
                Recipe singleUrRecipe = RecipeInterpreter.ConvertFileToRecipe(log, "Recipes/SingleURRecipe.json");
                Recipe effimatRecipe = RecipeInterpreter.ConvertFileToRecipe(log, "Recipes/EffimatRecipe.json");
                Recipe Cell1RightCell2Left = RecipeInterpreter.ConvertFileToRecipe(log, "Recipes/Cell1RightCell2LeftLab.json");
                Recipe Cell1LeftCell2Right = RecipeInterpreter.ConvertFileToRecipe(log, "Recipes/Cell1LeftCell2RightLab.json");
                Recipe Cell1RightCell2LeftStub = RecipeInterpreter.ConvertFileToRecipe(log, "Recipes/Cell1RightCell2LeftStub.json");
                Recipe Cell1LeftCell2RightStub = RecipeInterpreter.ConvertFileToRecipe(log, "Recipes/Cell1LeftCell2RightStub.json");

                cookbook.AddRecipe("drone", droneRecipe);
                cookbook.AddRecipe("UR", URRecipe);
                cookbook.AddRecipe("ACOPOStrak", ACOPOStrakRecipe);
                cookbook.AddRecipe("reset", resetRecipe);
                cookbook.AddRecipe("droneWithoutEffimat", droneWithoutEffimatRecipe);
                cookbook.AddRecipe("UR1", singleUrRecipe);
                cookbook.AddRecipe("effimat", effimatRecipe);
                cookbook.AddRecipe("Cell1RightCell2Left", Cell1RightCell2Left);
                cookbook.AddRecipe("Cell1LeftCell2Right", Cell1LeftCell2Right);
                
            }
            catch (I4Exception ex)
            {
                log.LogError(GetType(), ex, "A recipe could not be interpreted, exception: {exceptionMessage}",
                    ex.Message);
            }
        }

        public void StopRecipeExecution()
        {
            chefs.ForEach(c => c.SetStopExecution(true));
        }

        public void VerifyCapabilitiesReady(string orderId)
        {
            if (orderToBeStarted?.OrderId == orderId)
            {
                log.LogDebug(GetType(), "Capabilities are now ready, starting execution of order: {orderId}", orderId);
                StartOrderExecution(orderToBeStarted);
                orderToBeStarted = null;
            }
            else
            {
                log.LogError(GetType(), "Could not verify order, current order to be verified: {orderInVerification}, received: {orderId}", orderToBeStarted?.OrderId, orderId);
            }
        }

        public Order InitializeOrder(string orderId, string recipeName, int amount = 1)
        {
            return InitializeOrderWithRecipe(orderId, cookbook.GetRecipe(recipeName), amount);
        }

        public Order InitializeOrderWithRecipe(string orderId, Recipe recipe, int amount = 1)
        {
            log.LogDebug(GetType(), "Received order {order}, current orderQueue size: {orderQueue}", orderId, orderQueue.Count);
            Order order = new Order()
            {
                OrderId = orderId,
                Recipe = recipe,
                TotalAmount = amount
            };
            order.OnSuccess += OnOrderHandled;
            order.OnFailure += OnOrderHandled;
            if (orderQueue.Count <= 0 && orderToBeStarted == null &&
                (currentOrder == null || currentOrder.TotalAmount == currentOrder.GetAmountProduced()))
            {
                log.LogDebug(GetType(), "starting order {order} directly, as the order queue is empty", order);

                SetupForOrderExecution(order);
            }
            else
            {
                log.LogDebug(GetType(), "Enqueuing order {order}, as currently executing order: {currentOrder}",order, currentOrder);
                orderQueue.Enqueue(order);
            }
            return order;
        }


        private void SetupForOrderExecution(Order order)
        {
            JArray capabilities = new JArray();
            foreach (var step in order.Recipe.steps)
            {
                capabilities.Add(step.command.operation);
            }

            orderToBeStarted = order;
            capabilities = new JArray(capabilities.Distinct());
            JObject request = new JObject();
            request.Add("@id", Guid.NewGuid().ToString());
            request.Add("@type", "configuration_request");
            request.Add("orderId", order.OrderId);
            request.Add("aasOriginId", _originId);
            request.Add("aasTargetId", "i4.sdu.dk/Middleware/Configurator");
            request.Add("capabilities", capabilities);
            _producer.ProduceMessage(new List<string>(){"Configuration"}, request);
        }

        public void StartOrderExecution(Order order)
        {
            try
            {
                currentOrder = order;
                Queue<IChef> availableChefs = new Queue<IChef>(chefs.FindAll(c => !c.isCurrentlyExecutingRecipe()));
                log.LogInformation(GetType(),
                    "Starting order with orderId: {orderId}, recipe: {recipeName} and amount: {amount}, currently {availableChefs} chefs available", order.OrderId,
                    order.Recipe.recipeName, order.TotalAmount, availableChefs.Count);
                for (int i = 0; i < order.TotalAmount; i++)
                {
                    if (availableChefs.Count <= 0) break;
                    StartNewProduction(availableChefs.Dequeue());
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void OnCompleted()
        {
            throw new NotImplementedException();
        }

        public void OnError(Exception error)
        {
            throw new NotImplementedException();
        }

        public void OnNext(AvailabilityNotification value)
        {
            log.LogDebug(GetType(),
                "chef now available again, currently available chefs: {availableChefs}, there are {productsLeft} left to produce for order: {orderId}", chefs.FindAll(c => !c.isCurrentlyExecutingRecipe()).Count, currentOrder == null ? 0 : currentOrder?.TotalAmount - (currentOrder?.GetAmountProduced() + currentOrder?.GetAmountInProduction()), currentOrder == null ? "no current order" : currentOrder.OrderId);

            lock (this)
            {
                if (currentOrder?.TotalAmount >
                    currentOrder?.GetAmountProduced() + currentOrder?.GetAmountInProduction())
                {
                    StartNewProduction(value.Chef);
                }
            }
        }

        private void StartNewProduction(IChef chef)
        {
            var product = new Product()
            {
                id = Guid.NewGuid().ToString(),
                orderId = currentOrder.OrderId
            };
            log.LogDebug(GetType(), "Starting the production of new product {productId}, for order {orderId}", product.id, currentOrder.OrderId);
            product.OnSuccess += OnProductCompletion;
            product.OnFailure += OnProductFailure;
            currentOrder.AddToInProduction(product);
            log.LogDebug(GetType(),
                "Starting new chef for order: {orderId}, and product: {productId} count of active chefs: {activeChefCount}", currentOrder.OrderId, product.id, chefs.FindAll(c => c.isCurrentlyExecutingRecipe() == true).Count + 1);
            chef.ExecuteRecipe(currentOrder.Recipe, product);
        }

        public void OnProductCompletion(Product product)
        {
            log.LogDebug(GetType(), "Product {productId} for order {orderId} finished", product.id, product.orderId);
            if (currentOrder?.OrderId == product.orderId)
            {
                currentOrder?.RemoveFromInProduction(product);
                currentOrder?.AddToProduced(product);
                log.LogDebug(GetType(), "Execution time for product {productId}: {productExecutiontime} milliseconds", product.id, product.endTime.Subtract(product.startTime).TotalMilliseconds);

                log.LogDebug(GetType(), "Product Log: {productLog}", string.Join("\n", product.logs));
            }
        }

        public void OnProductFailure(Product product)
        {
            log.LogDebug(GetType(), "Production of product {productId} failed, error: {productError}", product.id, product.error);
            if (currentOrder.OrderId == product.orderId)
            {
                currentOrder.AddToFailed(product);
                foreach (var chef in chefs)
                {
                    chef.SetStopExecution(true);
                }
            }
        }

        public void OnOrderHandled(Order order)
        {
            log.LogDebug(GetType(), "Order {orderId} finished, produced {amount} products" , order.OrderId, order.TotalAmount);
            if (orderQueue.Count <= 0)
            {
                log.LogDebug(GetType(), "The order queue is empty");
                currentOrder = null;
            }
            else
            {
                Order newOrder = orderQueue.Dequeue();
                log.LogDebug(GetType(), "Starting on new order: {order}, current queue size: {orderQueueSize}", newOrder, orderQueue.Count);
                if (_without_configurator)
                {
                    StartOrderExecution(newOrder);
                }
                else
                {
                    SetupForOrderExecution(newOrder);
                }
            }
        }
    }
}