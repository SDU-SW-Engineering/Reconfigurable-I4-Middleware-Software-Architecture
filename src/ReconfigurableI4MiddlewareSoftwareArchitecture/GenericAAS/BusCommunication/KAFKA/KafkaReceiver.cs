using Confluent.Kafka;
using I4ToolchainDotnetCore.Logging;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GenericAAS.BusCommunication.KAFKA
{
    /// <summary>
    /// An implementation, that is based on a single kafka consumer, that is able to handle multiple subscriptions - downside is handling of switching between topics,
    /// while no EOF message is received, it will continue listening to the topic, and only switching on receival of EOF. Might be the wrong choice, when one topic
    /// will receive a continuous stream of messages.
    /// </summary>
    public class KafkaReceiver : IKafkaReceiver
    {
        private bool requestSubscriptionUpdate = false;
        private II4Logger _log;
        private string _host;
        private string _port;
        private IKafkaProducer _producer;
        private string _groupId;
        private Dictionary<string, Action<string, BusMessage>> subscriptionHandlers;
        public KafkaReceiver(string host, string port, string groupId, II4Logger log, IKafkaProducer producer)
        {
            _log = log;
            _host = host;
            _port = port;
            _groupId = groupId;
            _producer = producer;
            subscriptionHandlers = new Dictionary<string, Action<string, BusMessage>>();
        }
        public void AddSubscription(string topic, Action<string, BusMessage> msgHandler)
        {
            _log.LogDebug(GetType(), "Adding subscription: {subscription}", topic);
            subscriptionHandlers.Add(topic, msgHandler);
            requestSubscriptionUpdate = true;
            _producer.ProduceMessage(topic, new JObject(){["test"]="test"});
        }

        public void RemoveSubscription(string topic)
        {
            
            if (subscriptionHandlers.Remove(topic))
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
            return this.subscriptionHandlers.Keys.ToList();
        }

        private void UpdateSubscriptions(IConsumer<Ignore, string> consumer)
        {
            if (requestSubscriptionUpdate) {
                foreach(string topic in GetSubscriptions())
                {
                    _log.LogDebug(GetType(), "now subscribed to topic: {topic}", topic);
                    consumer.Subscribe(topic);
                }
                
                requestSubscriptionUpdate = false;
            };
        }

        private void HandleConsumeResult(ConsumeResult<Ignore, string> result)
        {
            _log.LogDebug(GetType(), $"Received Message from Kafka: {result.Message.Value}");
            if (subscriptionHandlers.ContainsKey(result.Topic))
            {
                subscriptionHandlers[result.Topic](result.Topic, new BusMessage()
                {
                    Topic = result.Topic,
                    Message = JObject.Parse(result.Message.Value),
                    TimeStamp = result.Message.Timestamp.UtcDateTime,
                    Raw = JsonConvert.SerializeObject(result)
                });
            }
        }

        public async Task Run()
        {
            var conf = new ConsumerConfig
            {
                GroupId = _groupId,
                BootstrapServers = _host + ":" + _port,
                AutoOffsetReset = AutoOffsetReset.Earliest,
            };
            await Task.Run(() =>
            {
                _log.LogDebug(GetType(), "Starting kafka subscriber with groupId {groupId}", _groupId);
                using (var consumer = new ConsumerBuilder<Ignore, string>(conf).Build())
                {
                    Console.CancelKeyPress += (_, e) => { e.Cancel = true; };
                    try
                    {
                        while (true)
                        {
                            _log.LogDebug(GetType(), "waiting");
                            UpdateSubscriptions(consumer);
                            try
                            {
                                var consumeResult = consumer.Consume(5000);
                                if (consumeResult != null)
                                {
                                    _log.LogDebug(GetType(), "Received message: " + consumeResult.Message.Value);
                                    HandleConsumeResult(consumeResult);
                                }

                            }
                            catch (ConsumeException e)
                            {
                                _log.LogError(GetType(), "Error occured while consuming: {error}", e.Error.Reason);
                            }
                            catch (JsonReaderException e)
                            {
                                _log.LogError(GetType(), "Error occured while interpreting json: {error}", e.Message);
                            }
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        consumer.Close();
                    }
                }
            });
            
        }
    }
}
