using Configurator.DomainObjects;
using I4ToolchainDotnetCore.Communication.Kafka;
using I4ToolchainDotnetCore.Communication.Message;
using I4ToolchainDotnetCore.Logging;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using Configurator.ConfigurationTools;

namespace Configurator.Kafka
{
    public class KafkaMessageHandler : IKafkaMessageHandler
    {
        private II4Logger _log;
        private IConfiguration _config;
        private string aasOriginId;
        private IConfigurator _configurator;
        private readonly IConfigurationInitializer _initializer;

        public KafkaMessageHandler(IConfiguration config, II4Logger log, IConfigurator configurator, IConfigurationInitializer initializer)
        {
            _configurator = configurator;
            _initializer = initializer;
            _log = log;
            _config = config;
            aasOriginId = _config.GetValue<string>("AAS_ORIGIN_ID");
        }
        public void HandleMessage(KafkaMessage msg)
        {
            _log.LogDebug(GetType(), "Received message on topic: {topic}, with content: {msg}", msg.Topic, msg.Message);
            try
            {
                var convertedMessage = ConvertMessage(msg.Message);
                if (!IsRightTarget(convertedMessage))
                    throw new ArgumentException(
                        $"received message with target: {convertedMessage.aasTargetId}, was not correct target");
                if (convertedMessage is ConfigurationRequest configurationRequest)
                {
                    _log.LogInformation(GetType(), "Message was Configuration Request");
                    _configurator.HandleConfigurationRequest(configurationRequest);
                }
                else if (convertedMessage is InitializationRequest iM)
                {
                    _log.LogDebug(GetType(), "Message was Initialization request");
                    _configurator.InitializePreparedConfiguration(iM);
                }
                else if (convertedMessage is AssetStopResponse assetStopResponse)
                {
                    _log.LogDebug(GetType(), "Message was Initialization request");
                    _initializer.ConfirmAssetStop(assetStopResponse);
                }
                else if (convertedMessage is AssetStartResponse assetStartResponse)
                {
                    _log.LogDebug(GetType(), "Message was Initialization request");
                    _initializer.ConfirmAssetStart(assetStartResponse);
                }
            }
            catch (ArgumentException ex)
            {
                _log.LogInformation(GetType(), "Did not handle message -> {message}", ex.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

        }

        public Message ConvertMessage(string msg)
        {
            try
            {
                Message returnMsg = null;
                var message = JObject.Parse(msg);
                string type = message.Value<string>("@type");
                //_log.LogDebug(GetType(), "Expecting: {firstType} and {secondType}", nameof(MSG_TYPE.CONFIGURATION_REQUEST), nameof(MSG_TYPE.CONFIGURATION_RESPONSE));
                switch (type?.ToUpper())
                {
                    case nameof(MSG_TYPE.CONFIGURATION_REQUEST):
                        returnMsg = JsonConvert.DeserializeObject<ConfigurationRequest>(msg);
                        break;
                    case nameof(MSG_TYPE.CONFIGURATION_RESPONSE):
                        returnMsg = JsonConvert.DeserializeObject<ConfigurationResponse>(msg);
                        break;
                    case "INITIALIZATION_REQUEST":
                        returnMsg = JsonConvert.DeserializeObject<InitializationRequest>(msg);
                        break;
                    case "STOP":
                        returnMsg = JsonConvert.DeserializeObject<AssetStopResponse>(msg);
                        break;
                    case "INITIAL_HEARTBEAT":
                        returnMsg = JsonConvert.DeserializeObject<AssetStartResponse>(msg);
                        break;
                    default:
                        throw new ArgumentException($"received message-type '{type}' was not expected");
                }
                return returnMsg;
            }
            catch (JsonReaderException e)
            {
                _log.LogError(GetType(), "error parsing message: {msg}", msg);
                throw new ArgumentException("could not interpret the message");
            }
        }

        private bool IsRightTarget(Message message)
        {
            //_log.LogDebug(GetType(), "comparing {target} with {origin}", message.aasTargetId, aasOriginId);
            return message.aasTargetId.Equals(aasOriginId);
        }
    }
}
