using System;
using GenericAAS.AssetCommunication.MQTT;
using GenericAAS.AssetCommunication.OPCUA;
using I4ToolchainDotnetCore.Logging;
using Newtonsoft.Json.Linq;

namespace GenericAAS.AssetCommunication
{
    public class AssetClientFactory : IAssetClientFactory
    {
        private II4Logger _log;
        public AssetClientFactory(II4Logger log)
        {
            _log = log;
        }

        public IAssetClient GetAssetClient(JObject config)
        {
            try
            {
                PROTOCOL_TYPE proto_type = ExtractType(config);
                switch (proto_type)
                {
                    case PROTOCOL_TYPE.MQTT: return CreateMQTTClient(config);
                    case PROTOCOL_TYPE.OPCUA: return CreateOPCUAClient(config);
                    default: throw new ArgumentException($"Could not identify client id {proto_type}");
                }

            }
            catch (ArgumentException e)
            {
                throw new ArgumentException($"Could not create asset client <- {e.Message}");
            }
        }

        private IAssetClient CreateMQTTClient(JObject config)
        {
            return new MQTTAssetClient(config, _log);
        }
        private IAssetClient CreateOPCUAClient(JObject config)
        {
            return new OPCUAAssetClient(config, _log);
        }
        

        private PROTOCOL_TYPE ExtractType(JObject config)
        {
            _log.LogDebug(GetType(), config.ToString());
            if (config.TryGetValue("type", out JToken typeToken))
            {
                if (Enum.TryParse(typeToken.Value<string>(), out PROTOCOL_TYPE proto_type))
                {
                    return proto_type;
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
    }
}

