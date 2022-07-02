using I4ToolchainDotnetCore.Communication.Kafka;
using I4ToolchainDotnetCore.Logging;
using I4ToolchainDotnetCore.ServiceLayer.Operation;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Orchestrator.Adapter;
using Orchestrator.Adapter.RecipeInterpretation;
using Orchestrator.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Orchestrator.Service.Operations
{
    public class StartSequenceFromMessageOperation : IOperation
    {

        private static readonly string KEYWORD = "fromMessage";
        private readonly ICoordinator _coordinator;
        private readonly II4Logger _log;
        private int STANDARD_AMOUNT = 1;
        private IKafkaProducer _producer;
        private IConfiguration _config;
        private string aasOriginId;
        public StartSequenceFromMessageOperation(IKafkaProducer producer, II4Logger log, ICoordinator coordinator, IConfiguration config)
        {
            _config = config;
            aasOriginId = config.GetValue<string>("AAS_ORIGIN_ID") ?? "i4.sdu.dk/Middleware/Orchestrator";

            _producer = producer;
            this._log = log;
            _coordinator = coordinator;
        }

        public IOperation Clone()
        {
            return new StartSequenceFromMessageOperation(_producer, _log, _coordinator, _config);
        }

        public string GetOperationKeyword()
        {
            return KEYWORD;
        }

        public string GetStatus()
        {
            throw new NotImplementedException();
        }

        public async Task HandleOperation(String topic, OperationMessage message)
        {
            DateTime start = DateTime.Now;
            _log.LogInformation(GetType(), "Starting execution of operation {operationKeyword} based on order: {order}", KEYWORD, message.orderId);
            if (message.parameters.Count < 1 || !message.parameters.ContainsKey("recipe"))
            {
                _log.LogError(GetType(), "Missing parameters, got {parameters}", string.Join(", ", message.parameters));
                throw new MissingParameterException($"Missing parameters, either no parameters, or missing recipe, received: {message.ToString()}");
            }
            else
            {
                message.parameters.TryGetValue("order", out JObject receivedOrder);
                receivedOrder.TryGetValue("recipe", out var jsonRecipe);
                var amount = DetermineAmount(receivedOrder);
                Recipe recipe = RecipeInterpreter.ConvertMessageToRecipe(_log, ((JObject)jsonRecipe)?.ToString());
                JObject response = new JObject();
                try
                {
                    _coordinator.InitializeOrderWithRecipe(message.orderId, recipe, amount: amount);
                    response = new JObject();
                    response.Add("success", true);
                } catch(OrderNotExecutedException ex)
                {
                    _log.LogError(GetType(), ex, "The recipe {recipeName} was not executed, exception: {exceptionMessage}", jsonRecipe, ex.Message);
                    response = new JObject();
                    response.Add("success", false);
                    response.Add("description", ex.Message);
                }
                JObject returnValue = new JObject();
                returnValue.Add("@id", Guid.NewGuid().ToString());
                returnValue.Add("@type", "response");
                returnValue.Add("operationId", message.id);
                returnValue.Add("aasOriginId", aasOriginId);
                returnValue.Add("aasTargetId", message.aasOriginId);
                returnValue.Add("response", response);
                _producer.ProduceMessage(new List<string>() { topic }, returnValue);
                _log.LogInformation(GetType(), "The execution of recipe {operationKeyword} with amount {amount} took {operationDuration} milliseconds", KEYWORD, amount, DateTime.Now.Subtract(start).TotalMilliseconds);

            }

        }


        private int DetermineAmount(JObject message)
        {
            Console.WriteLine("determining amount");
            int amount = STANDARD_AMOUNT;
            try
            {
                if (message.Count >= 1 && message.ContainsKey("amount"))
                {
                    message.TryGetValue("amount", out var stringAmount);
                    amount = int.Parse((string) stringAmount);
                    _log.LogDebug(GetType(), "Extracting parameter, got {amount}", amount);
                }
            }
            catch (JsonReaderException ex)
            {
                _log.LogError(GetType(), "Could not determine amount, using standard");
            }
            return amount;
        }
    }
}
