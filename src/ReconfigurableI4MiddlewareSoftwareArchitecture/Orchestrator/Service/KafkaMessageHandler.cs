using I4ToolchainDotnetCore.Communication.Kafka;
using I4ToolchainDotnetCore.Communication.Message;
using I4ToolchainDotnetCore.Logging;
using I4ToolchainDotnetCore.ServiceLayer.Operation;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Orchestrator.DomainObjects;
using System;
using System.Collections.Generic;
using System.Text;
using Orchestrator.Adapter;
using VDS.RDF.Query.Expressions.Functions.Sparql;

namespace Orchestrator.Service
{
    public class KafkaMessageHandler : IKafkaMessageHandler
    {
        private IOperationManager _operationManager;
        private readonly IConfiguration _config;
        private string aasOriginId;
        private II4Logger _log;
        private IConfigurationHandler _configHandler;
        private ICoordinator _coordinator;
        private IKafkaProducer _producer;
        public KafkaMessageHandler(IOperationManager operationManager, IConfiguration config, II4Logger log, IConfigurationHandler configHandler, ICoordinator coordinator, IKafkaProducer producer)
        {
            _configHandler = configHandler;
            _config = config;
            _log = log;
            aasOriginId = _config.GetValue<string>("AAS_ORIGIN_ID");
            _log.LogDebug(GetType(), "starting with originId: {originid}", aasOriginId);
            _operationManager = operationManager;
            _coordinator = coordinator;
            _producer = producer;
        }
        public void HandleMessage(KafkaMessage msg)
        {
            //_log.LogDebug(GetType(), "topic: {topic}, content: {msg}", msg.Topic, msg.Message);
            try
            {
                var convertedMessage = ConvertMessage(msg.Message);
                
                if (!IsRightTarget(convertedMessage)) throw new ArgumentException($"received message with target: { convertedMessage.aasTargetId}, was not correct target");
                if (convertedMessage is OperationMessage oM)
                {
                    _log.LogDebug(GetType(), "converted: {converted}", oM.parameters);
                    _log.LogInformation(GetType(), "operation: {operation}", oM.operation);
                    _operationManager.StartOperation(msg.Topic, oM);
                }
                else if (convertedMessage is ResponseMessage rM)
                {
                    _log.LogDebug(GetType(), "response: {response}", rM.response);
                } 
                else if(convertedMessage is CapabilityRequestMessage cM)
                {
                    _configHandler.HandleConfigurationRequest(cM);
                }
                else if(convertedMessage is CapabilityResponseMessage capabilityResponse)
                {
                    JObject request = new JObject();
                    request.Add("@id", Guid.NewGuid().ToString());
                    request.Add("@type", "initialization_request");
                    request.Add("aasOriginId", aasOriginId);
                    request.Add("aasTargetId", "i4.sdu.dk/Middleware/Configurator");
                    request.Add("orderId", capabilityResponse.orderId);
                    _producer.ProduceMessage(new List<string>(){"Configuration"}, request);
                }
                else if(convertedMessage is CapabilitiesReadyMessage capabilitiesReady)
                {
                    _coordinator.VerifyCapabilitiesReady(capabilitiesReady.orderId);
                }
            }
            catch (ArgumentException ex)
            {
                _log.LogInformation(GetType(), "Did not handle message -> {message}", ex.Message);
            }

        }

        public Message ConvertMessage(string msg)
        {
            try
            {
                Message returnMsg = null;
                var message = JObject.Parse(msg);
                string type = message.Value<string>("@type");
                switch (type)
                {
                    case "operation":
                        returnMsg = JsonConvert.DeserializeObject<OperationMessage>(msg);
                        break;
                    case "response":
                        returnMsg = JsonConvert.DeserializeObject<ResponseMessage>(msg);
                        break;
                    case "capability_request":
                        returnMsg = JsonConvert.DeserializeObject<CapabilityRequestMessage>(msg);
                        break;
                    case "capability_response":
                        returnMsg = JsonConvert.DeserializeObject<CapabilityResponseMessage>(msg);
                        break;
                    case "capabilities_ready":
                        returnMsg = JsonConvert.DeserializeObject<CapabilitiesReadyMessage>(msg);
                        break;
                    default:
                        throw new ArgumentException($"received message-type '{type}' was not expected");
                }
                return returnMsg;
            }
            catch (JsonReaderException e)
            {
                _log.LogError(GetType(), e, "error parsing message: {msg}, error: {parsingError}", msg, e.Message);
                throw new ArgumentException("could not interpret the message");
            }
        }

        private bool IsRightTarget(Message message)
        {
            _log.LogDebug(GetType(), "comparing {target} with {origin}", message.aasTargetId, aasOriginId);
            return message.aasTargetId.Equals(aasOriginId);
        }
    }
}
