using I4ToolchainDotnetCore.Communication.Kafka;
using I4ToolchainDotnetCore.Logging;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Orchestrator.DomainObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace Orchestrator.Service
{
    public class ConfigurationHandler : IConfigurationHandler
    {
        private IConfiguration _config;
        private IKafkaProducer _producer;
        private string configuratorTopic;
        private string aasOriginId;
        private II4Logger _log;
        public ConfigurationHandler(IConfiguration config, IKafkaProducer producer, II4Logger log)
        {
            _log = log;
            _config = config;
            configuratorTopic = _config.GetValue<string>("CONFIGURATOR_TOPIC") ?? "Configuration";
            aasOriginId = _config.GetValue<string>("AAS_ORIGIN_ID") ?? "i4.sdu.dk/Middleware/Configurator";
            _producer = producer;
        }

        public void HandleConfigurationRequest(CapabilityRequestMessage msg)
        {
            _log.LogDebug(GetType(), "handling configuration request with capabilities: {capabilities}", string.Join(", ", msg.capabilities));
            msg.type = "configuration_request";
            msg.aasTargetId = "i4.sdu.dk/Middleware/Configurator";
            msg.aasOriginId = aasOriginId;
            _producer.ProduceMessage(new List<string>() { configuratorTopic }, JObject.Parse(JsonConvert.SerializeObject(msg)));
        }
    }
}
