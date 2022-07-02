using System;
using System.Collections.Generic;
using System.Linq;
using Configurator.DomainObjects;
using Configurator.Model;
using I4ToolchainDotnetCore.Communication.Kafka;
using I4ToolchainDotnetCore.Logging;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;

namespace Configurator.ConfigurationTools
{
    public class Configurator : IConfigurator
    {
        private readonly II4Logger _log;
        private readonly IKafkaProducer _producer;
        private readonly IConfigurationFinder _finder;
        private readonly IConfigurationAssessor _assessor;
        private readonly IConfigurationInitializer _initializer;
        private readonly IConfigurationMapper _mapper;
        private PreparedConfiguration preparedConfiguration;
        private string aasOriginId;
        private DateTime start;
        public Configurator(II4Logger log, IConfiguration config, IKafkaProducer producer, IConfigurationFinder finder, IConfigurationAssessor assessor, IConfigurationInitializer initializer, IConfigurationMapper mapper)
        {
            _log = log;
            _producer = producer;
            _finder = finder;
            _assessor = assessor;
            _initializer = initializer;
            _mapper = mapper;
            aasOriginId = config.GetValue<string>("AAS_ORIGIN_ID") ?? "i4.sdu.dk/Middleware/Configurator";

        }
        public void HandleConfigurationRequest(ConfigurationRequest request)
        {
            try
            {
                start = DateTime.Now;
                _log.LogDebug(GetType(), "Starting configuration for request: {configurationRequest} and orderId: {requestOrderId}", request.id, request.orderId);
                PrepareConfiguration(request);
            }
            catch (ArgumentNullException e)
            {
                _log.LogError(GetType(), e,
                    "could not start configuration request, no capabilities where found <- {error}", e.Message);
            }
            catch (ArgumentException e)
            {
                _log.LogError(GetType(), e, "Argument exception: {msg}", e.Message);
            }
            catch (Exception e)
            {
                _log.LogError(GetType(), e, "some error: {msg}", e.Message);
            }
        }

        private void PrepareConfiguration(ConfigurationRequest request)
        {
            var capabilitySets = _finder.Find(request);
            var set = _assessor.FindBestCapabilitySet(capabilitySets, request.capabilities.First());
            preparedConfiguration = new PreparedConfiguration()
                {Config = _mapper.MapCapabilitysetToAssets(set), OrderId = request.orderId};
           
            JObject readyMessage = new JObject();
            readyMessage.Add("@id", Guid.NewGuid().ToString());
            readyMessage.Add("@type", "capability_response");
            readyMessage.Add("operationId", request.id);
            readyMessage.Add("orderId", request.orderId);
            readyMessage.Add("aasOriginId", aasOriginId);
            readyMessage.Add("aasTargetId", request.aasOriginId);
            _producer.ProduceMessage(new List<string>(){"Executions"}, readyMessage);
            _log.LogInformation(GetType(), "Preparing configuration for orderid: {id} took {prepareTime} milliseconds", request.orderId, DateTime.Now.Subtract(start).TotalMilliseconds);
        }

        public void InitializePreparedConfiguration(InitializationRequest request)
        {
            _log.LogDebug(GetType(), "Starting Initialization of configuration for orderid: {id}", request.orderId);
            if (preparedConfiguration.OrderId == request.orderId)
            {
                _initializer.InitializeConfiguration(preparedConfiguration.Config, request);
            }
            else
            {
                _log.LogError(GetType(), "Could not start initialization of configuration for orderId {notStartedOrderId}, was not the one that was prepared.", request.orderId);
            }
        }
        private void SendRecipe()
        {
            // JObject returnValue = new JObject();
            // returnValue.Add("@id", Guid.NewGuid().ToString());
            // returnValue.Add("@type", "operation");
            // returnValue.Add("operationId", configRequest.id);
            // returnValue.Add("orderId", configRequest.orderId);
            // returnValue.Add("aasOriginId", aasOriginId);
            // returnValue.Add("aasTargetId", configRequest.aasOriginId);
            // returnValue.Add("operation", "fromMessage");
            // returnValue.Add("parameters", new JObject() { { "recipe", recipe.ToString()} });
            // _producer.ProduceMessage(new List<string>() { "Executions" }, returnValue);
        }
    }
}