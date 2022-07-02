using System;
using GenericAAS.BusCommunication.KAFKA;
using I4ToolchainDotnetCore.Logging;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;

namespace GenericAAS.BusCommunication
{
    public class BusClientFactory : IBusClientFactory
    {
        private II4Logger _log;
        public BusClientFactory(IConfiguration config, II4Logger log)
        {
            _log = log;
        }

        public IBusClient GetBusClient(JObject config)
        {
            try
            {
                BUS_TYPE bus_type = ExtractType(config);
                switch (bus_type)
                {
                    case BUS_TYPE.KAFKA: return CreateKafkaClient(config);
                    default: return CreateKafkaClient(config);
                }

            }
            catch (ArgumentException e)
            {
                throw new ArgumentException($"Could not create bus client <- {e.Message}");
            }

        }
        private BUS_TYPE ExtractType(JObject config)
        {
            _log.LogDebug(GetType(), config.ToString());
            if(config.TryGetValue("type", out JToken typeToken))
            {
                if (Enum.TryParse(typeToken.Value<string>(), out BUS_TYPE bus_type)){
                    return bus_type;
                }
                else
                {
                    throw new ArgumentException("could not find type parameter");
                }
                
            }
            else
            {
                throw new ArgumentException("Could not find description of type");
            }
        }

        private IBusClient CreateKafkaClient(JObject config)
        {
            try
            {
                return new KafkaClient(config, _log);
            }
            catch(ArgumentException e)
            {
                throw new ArgumentException($"Could not create kafka client <- {e.Message}");
            }
        }
    }
}

