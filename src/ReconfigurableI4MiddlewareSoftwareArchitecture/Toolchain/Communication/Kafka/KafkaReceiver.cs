using Confluent.Kafka;
using I4ToolchainDotnetCore.Logging;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace I4ToolchainDotnetCore.Communication.Kafka
{
    /// <summary>
    /// An implementation, that is based on a single kafka consumer, that is able to handle multiple subscriptions - downside is handling of switching between topics,
    /// while no EOF message is received, it will continue listening to the topic, and only switching on receival of EOF. Might be the wrong choice, when one topic
    /// will receive a continuous stream of messages.
    /// </summary>
    public class KafkaReceiver : IKafkaReceiver
    {
        private readonly IConfiguration _config;
        private readonly IKafkaMessageHandler _msgHandler;
        private Dictionary<string, CancellationTokenSource> topicSubCancellationTokenSources;
        private string KAFKA_GROUP_ID = "KAFKA_RECEIVER";
        private List<string> currentSubscriptions;
        private bool requestSubscriptionUpdate = false;
        private II4Logger _log;
        public KafkaReceiver(IConfiguration config, IKafkaMessageHandler msgHandler, II4Logger log)
        {
            _log = log;
            _config = config;
            _msgHandler = msgHandler;
            currentSubscriptions = new List<string>();
            KAFKA_GROUP_ID = _config.GetValue<string>("KAFKA_GROUP_ID");
            topicSubCancellationTokenSources = new Dictionary<string, CancellationTokenSource>();
        }
        public void AddSubscription(string topic)
        {
            _log.LogDebug(GetType(), "Adding subscription: {subscription}", topic);
            currentSubscriptions.Add(topic);
            requestSubscriptionUpdate = true;
        }

        public void RemoveSubscription(string topic)
        {
            if (currentSubscriptions.Remove(topic))
            {
                requestSubscriptionUpdate = true;
                _log.LogDebug(GetType(), "Removing subscription: {subscription}", topic);
            }
            else
            {
                throw new ArgumentException($"Could not remove {topic}, was not found");
            }
            
        }

        public List<string> GetSubscriptions()
        {
            return this.currentSubscriptions;
        }

        public void Run()
        {
            var conf = new ConsumerConfig
            {
                GroupId = KAFKA_GROUP_ID,
                BootstrapServers = _config.GetValue<string>("KAFKA_HOST") ?? "192.168.0.105:9092",
                AutoOffsetReset = AutoOffsetReset.Earliest,
            };
            using (var consumer = new ConsumerBuilder<Ignore, string>(conf).Build())
            {
                consumer.Subscribe("config");
                Console.CancelKeyPress += (_, e) =>
                {
                    e.Cancel = true;
                };
                try
                {
                    while (true)
                    {
                        if (requestSubscriptionUpdate) { consumer.Subscribe(currentSubscriptions); requestSubscriptionUpdate = false; };
                        try
                        {
                            var consumeResult = consumer.Consume(1);
                            if (consumeResult != null)
                            {
                                var msg = new KafkaMessage()
                                {
                                    Topic = consumeResult.Topic,
                                    Message = consumeResult.Message.Value,
                                    Timestamp = consumeResult.Message.Timestamp.UtcDateTime,
                                    Raw = JsonConvert.SerializeObject(consumeResult)
                                };
                                _msgHandler.HandleMessage(msg);
                            }

                        }
                        catch (ConsumeException e)
                        {
                            _log.LogError(GetType(), "Error occured while consuming: {error}", e.Error.Reason);
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    consumer.Close();
                }
            }
        }
    }
}
