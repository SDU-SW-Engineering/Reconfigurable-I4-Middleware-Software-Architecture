using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GenericAAS.DataModel;
using GenericAAS.Exceptions;
using I4ToolchainDotnetCore.Logging;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using Newtonsoft.Json.Linq;

namespace GenericAAS.AssetCommunication.MQTT
{
    public class MQTTAssetClient : IAssetClient
    {
        private II4Logger _log;
        private IMqttFactory factory;
        private string host;
        private string port;
        private string id;
        private string clientId;
        private static IMqttClient mqttClient;
        private List<IConditionHandler> conditionHandlers;
        private List<string> initialSubscriptions;
        private ConcurrentDictionary<string, string> currentTopicValues;
        private CancellationTokenSource cts = new CancellationTokenSource();
        private Step currentlyExecutingStep;

        public MQTTAssetClient(JObject config, II4Logger log)
        {
            _log = log;
            host = ExtractString("host", config);
            port = ExtractString("port", config);
            id = ExtractString("id", config);
            clientId = ExtractString("client_id", config);
            initialSubscriptions = ExtractStringArray("requiredSubscriptions", config);
            conditionHandlers = new List<IConditionHandler>();
            currentTopicValues = new ConcurrentDictionary<string, string>();
        }

        private List<string> ExtractStringArray(string initialsubscriptions, JObject config)
        {
            if (config.TryGetValue(initialsubscriptions, out JToken topicsToken))
            {
                List<string> topicList = new List<string>();
                var topics = (JArray) topicsToken;
                foreach (string topic in topics)
                {
                    topicList.Add(topic);
                }

                return topicList;
            }

            throw new ArgumentException($"Could not find communication protocol parameters for initial subscriptions");
        }

        public PROTOCOL_TYPE GetProtocolType()
        {
            return PROTOCOL_TYPE.MQTT;
        }

        public void HandleStep(Step step)
        {
            try
            {
                currentlyExecutingStep = step;
                switch (step.Method.ToLower())
                {
                    case "publish":
                        HandlePublish(step);
                        break;
                    case "subscribe":
                        HandleSubscribe(step);
                        break;
                }

                currentlyExecutingStep = null;
            }
            catch (ArgumentException e)
            {
                currentlyExecutingStep = null;
                throw new StepNotExecutedException($"Could not handle step <- {e.Message}", e);
            }
            catch (AssetNotConnectedException e)
            {
                currentlyExecutingStep = null;
                throw new StepNotExecutedException($"Could not handle step <- {e.Message}", e);
            }
        }

        private void HandlePublish(Step step)
        {
            _log.LogDebug(GetType(), $"Starting publish step");

            if (!HasConnection()) throw new AssetNotConnectedException($"Could not publish message, was not connected");
            try
            {
                if (!step.Parameters.TryGetValue("topic", out string topic))
                    throw new ArgumentException($"Could not find topic");
                if (!step.Parameters.TryGetValue("message", out string message))
                    throw new ArgumentException($"Could not find message");
                SendMessage(topic, message);
            }
            catch (ArgumentException e)
            {
                throw new ArgumentException($"Could not publish <- {e.Message}", e);
            }
        }

        private void HandleSubscribe(Step step)
        {
            _log.LogDebug(GetType(), $"Starting subscribe step");
            if (!HasConnection()) throw new AssetNotConnectedException($"Could not subscribe, was not connected");
            try
            {
                if (!step.Parameters.TryGetValue("topic", out string topic))
                    throw new ArgumentException($"Could not find topic");
                if (!initialSubscriptions.Contains(topic))
                    throw new ArgumentException("Can not handle subscription, not subscribed to topic");
                
                IConditionHandler condition = new ConditionHandler(topic, step.Condition, _log);
                _log.LogDebug(GetType(), "Subscribing to {subscribeTopic}, expecting {expectedMessage}", topic, step.Condition.Value);
                condition.Initialize();
                conditionHandlers.Add(condition);
                UpdateConditions();
                while (!condition.IsSatisfied())
                {
                }
                conditionHandlers.Remove(condition);
                
                var reaction = condition.GetReaction();
                if (reaction == REACTION.ERROR) throw new StepNotExecutedException("Time ran out");
            }
            catch (ArgumentException e)
            {
                throw new ArgumentException($"Could not finish subscription <- {e.Message}", e);
            }
        }

        public bool HasConnection()
        {
            return mqttClient?.IsConnected ?? false;
        }

        public void Initialize()
        {
            Task task = Task.Run(async () =>
            {
                _log.LogDebug(GetType(), $"Starting initializing MQTT receiver");
                await InitializeMqttClient();
                foreach (var topic in initialSubscriptions)
                {
                    SubscribeToTopic(topic);
                }
                _log.LogDebug(GetType(), $"Finished initializing MQTT receiver");
            }, cts.Token);
        }

        public bool VerifySteps(List<Step> steps)
        {
            throw new NotImplementedException();
        }

        public async void SubscribeToTopic(string topic)
        {
            if (mqttClient.IsConnected)
            {
                _log.LogDebug(GetType(), $"Subscribing to MQTT topic {topic}");
                await mqttClient.SubscribeAsync(new MqttTopicFilterBuilder().WithTopic(topic).Build());
            }
            else
            {
                _log.LogError(GetType(), "could not subscribe to topic {topic}, because the client was not connected",
                    topic);
            }
        }

        private async Task ConnectToMQTT(string mqttHost, string mqttPort, string mqttClientId)
        {
            _log.LogDebug(GetType(), "Connecting to mqtt with broker: {broker}, and clientId {clientId}", mqttHost + ":" + mqttPort, mqttClientId);
            await Task.Run(async () =>
            {
                var options = new MqttClientOptionsBuilder()
                    .WithClientId(mqttClientId+new Random().Next())
                    .WithTcpServer(mqttHost
                        , int.Parse(mqttPort))
                    .Build();
                SetReceivingHandler();
                while (!mqttClient.IsConnected)
                {
                    Task currentExecution = Task.Delay(5000, new CancellationTokenSource().Token);
                    await currentExecution.ContinueWith(async task =>
                    {
                        try
                        {
                            mqttClient.ConnectAsync(options, CancellationToken.None).GetAwaiter().GetResult();
                            await StartHeartBeat();
                        }
                        catch (MQTTnet.Exceptions.MqttCommunicationTimedOutException ex)
                        {
                            _log.LogError(GetType(), ex,
                                "could not connect to MQTT broker with ip {ip} and port: {port}",
                                mqttHost, mqttPort);
                        }
                        catch (MQTTnet.Exceptions.MqttCommunicationException ex)
                        {
                            _log.LogError(GetType(), ex,
                                "could not connect to MQTT broker with ip {ip} and port: {port}",
                                mqttHost, mqttPort);
                        }
                    });
                }
            });
        }

        private async Task InitializeMqttClient()
        {
            factory = new MqttFactory();
            mqttClient = factory.CreateMqttClient();
            await ConnectToMQTT(host, port, clientId);
        }

        private string ExtractString(string key, JObject config)
        {
            if (config.TryGetValue(key, out JToken hostToken)) return hostToken.Value<string>();
            throw new ArgumentException($"Could not find value: {key} in configuration");
        }

        private async Task StartHeartBeat()
        {
            _log.LogDebug(GetType(), "Starting heartbeat");
            await Task.Run(async () =>
            {
                while (mqttClient.IsConnected)
                {
                    cts.Token.ThrowIfCancellationRequested();
                    await Task.Delay(5000, new CancellationTokenSource().Token).ContinueWith(task =>
                    {
                        _log.LogInformation(GetType(), "Still connected to MQTT Broker");
                    });
                }

                await InitializeMqttClient();
            }, cts.Token);
        }

        private void SetReceivingHandler()
        {
            mqttClient.UseApplicationMessageReceivedHandler(e =>
            {
                _log.LogDebug(GetType(),
                    $"Received MQTT message:{Encoding.UTF8.GetString(e.ApplicationMessage.Payload)} on topic {e.ApplicationMessage.Topic}");
                currentTopicValues[e.ApplicationMessage.Topic] = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
                UpdateConditions();
            });
        }

        private void UpdateConditions()
        {
            foreach (var topicValuePair in currentTopicValues)
            {
                foreach (var condition in conditionHandlers)
                {
                    condition.UpdateJSONValue(topicValuePair.Key,
                        topicValuePair.Value);
                }
            }
        }


        private void SendMessage(string topic, string content)
        {
            _log.LogDebug(GetType(), "sending message with content {messageContent} on topic {topic}", content, topic);
            if (mqttClient.IsConnected)
            {
                var message = new MqttApplicationMessageBuilder()
                    .WithRetainFlag(false)
                    .WithTopic(topic)
                    .WithPayload(content)
                    .WithExactlyOnceQoS()
                    .Build();
                try
                {
                    mqttClient.PublishAsync(message, CancellationToken.None);
                }
                catch (Exception e)
                {
                    throw new ArgumentException(
                        $"message {content} was not sent to topic {topic}, problem while publishing, with exception: {e.Message}",
                        e);
                }
            }
            else
            {
                throw new ArgumentException(
                    $"message {content} was not sent to topic {topic}, client is not connected");
            }
        }

        public JObject GetStatus()
        {
            return new JObject()
            {
                ["HasConnection"] = HasConnection(),
                ["Busy"] = currentlyExecutingStep != null,
                ["CurrentlyExecutingStep"] = currentlyExecutingStep?.Id
            };        }

        public string GetId()
        {
            return id;
        }
    }
}