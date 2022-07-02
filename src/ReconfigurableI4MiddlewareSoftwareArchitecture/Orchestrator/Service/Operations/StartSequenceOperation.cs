﻿using I4ToolchainDotnetCore.Communication.Kafka;
using I4ToolchainDotnetCore.Logging;
using I4ToolchainDotnetCore.ServiceLayer.Operation;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Orchestrator.Adapter;
using Orchestrator.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Orchestrator.DomainObjects;

namespace Orchestrator.Service.Operations
{
    class StartSequenceOperation : IOperation
    {

        private static readonly string KEYWORD = "fabricateDrone";
        private readonly ICoordinator _coordinator;
        private readonly II4Logger _log;
        private int STANDARD_AMOUNT = 1;
        private IKafkaProducer _producer;
        private string aasOriginId;
        private IConfiguration _config;
        private string operationId;
        private string targetId;
        private string topic;
        private DateTime start;
        private string orderId;
        public StartSequenceOperation(IKafkaProducer producer, II4Logger log, IConfiguration config, ICoordinator coordinator)
        {
            _config = config;
            aasOriginId = config.GetValue<string>("AAS_ORIGIN_ID") ?? "i4.sdu.dk/Middleware/Orchestrator";
            
            _log = log;
            _coordinator = coordinator;
            _producer = producer;
        }

        public IOperation Clone()
        {
            return new StartSequenceOperation(_producer, _log, _config, _coordinator);
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
            targetId = message.aasOriginId;
            operationId = message.id;
            this.topic = topic;
            orderId = message.orderId;
            start = DateTime.Now;
            _log.LogInformation(GetType(), "Starting execution of recipe {operationKeyword} based on order: {order}", KEYWORD, message.orderId);
            
            if (message.parameters.Count < 1 || !message.parameters.ContainsKey("order"))
            {
                _log.LogError(GetType(), "Missing parameters, got {parameters}", string.Join(", ", message.parameters));
                throw new MissingParameterException($"Missing parameters, either no parameters, or missing recipe, received: {message.ToString()}");
            }
            else
            {
                message.parameters.TryGetValue("order", out JObject receivedOrder);
                receivedOrder.TryGetValue("recipeName", out var recipeName);
                var amount = DetermineAmount(receivedOrder);
                try
                {
                    
                    Order order = _coordinator.InitializeOrder( message.orderId, (string) recipeName, amount);
                    order.OnSuccess += HandleSuccess;
                    order.OnFailure += HandleFailure;


                } catch(OrderNotExecutedException ex)
                {
                    
                    //throw new OperationNotExecutedException($"The operation {KEYWORD} was not executed properly -> {ex.Message}", ex);
                }
                
            }
            //_log.LogInformation(GetType(), "The execution of recipe {operationKeyword} with amount {amount} took {operationDuration} milliseconds", KEYWORD, amount, DateTime.Now.Subtract(start).TotalMilliseconds);

        }

        private void HandleSuccess(Order order)
        {
            _log.LogInformation(GetType(), "The execution of recipe {operationKeyword} for order {orderId} took {operationDuration} milliseconds", KEYWORD, order.OrderId, DateTime.Now.Subtract(start).TotalMilliseconds);
            JObject response = new JObject();
            response = new JObject();
            response.Add("success", true);
            SendReturnMessage(response);
        }
        
        private void HandleFailure(Order order)
        {
            _log.LogError(GetType(), "The order {orderId} was not executed, the following errors occured: {errors}", order.OrderId,  order.GetFailureDescription());
            JObject response = new JObject();
            response = new JObject();
            response.Add("success", false);
            response.Add("description", order.GetFailureDescription());
            SendReturnMessage(response);
            
        }

        private void SendReturnMessage(JObject response)
        {
            JObject returnValue = new JObject();
            returnValue.Add("@id", Guid.NewGuid().ToString());
            returnValue.Add("@type", "response");
            returnValue.Add("operationId", operationId);
            returnValue.Add("aasOriginId", aasOriginId);
            returnValue.Add("aasTargetId", targetId);
            returnValue.Add("orderId", orderId);
            returnValue.Add("response", response);
            _producer.ProduceMessage(new List<string>() { topic }, returnValue);
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
