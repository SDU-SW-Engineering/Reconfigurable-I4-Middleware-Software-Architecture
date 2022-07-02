using Confluent.Kafka;
using I4ToolchainDotnetCore.Logging;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace GenericAAS.BusCommunication.KAFKA
{
    /// <summary>
    /// An Implementation of a kafka receiver, that creates a new consumer for each subscription. Good for handling multiple topics, where one or more
    /// receive a continuous stream of messages - not so good, if a lot of different topics are subscribed to.
    /// </summary>
    public class KafkaMultiThreadReceiver : IKafkaReceiver
    {
        private readonly IConfiguration _config;
        private Dictionary<string, CancellationTokenSource> topicSubCancellationTokenSources;
        private II4Logger _log;
        private string _host;
        private string _port;
        private IKafkaProducer _producer;
        private string _groupId;
        public KafkaMultiThreadReceiver(string host, string port, string groupId, II4Logger log, IKafkaProducer producer)
        {
            _log = log;
            _host = host;
            _port = port;
            _groupId = groupId;
            _producer = producer;
            topicSubCancellationTokenSources = new Dictionary<string, CancellationTokenSource>();
        }
        public void AddSubscription(string topic, Action<string, BusMessage> msgHandler)
        {
            var conf = new ConsumerConfig
            {
                GroupId = _groupId,
                BootstrapServers = _host + ":" + _port,
                AutoOffsetReset = AutoOffsetReset.Latest,
            };
            _producer.ProduceMessage(topic, new JObject(){["test"]="test"});

            _log.LogDebug(GetType(), "subscribed to {topic}", topic);
            var cts = new CancellationTokenSource();
            Task task = Task.Run(() =>
            {
                using (var consumer = new ConsumerBuilder<Ignore, string>(conf).Build())
                {


                    consumer.Subscribe(topic);
                    Console.CancelKeyPress += (_, e) =>
                    {
                        e.Cancel = true; // prevent the process from terminating.
                        cts.Cancel();
                    };

                    try
                    {
                        while (true)
                        {

                            try
                            {
                                if (cts.Token.IsCancellationRequested) throw new OperationCanceledException("cancelled externally");
                                var consumeResult = consumer.Consume(cts.Token);
                                _log.LogDebug(GetType(), $"Received Message from Kafka: {consumeResult.Message.Value}");
                                msgHandler(consumeResult.Topic, new BusMessage()
                                {
                                    Topic = consumeResult.Topic,
                                    Message = JObject.Parse(consumeResult.Message.Value),
                                    TimeStamp = consumeResult.Message.Timestamp.UtcDateTime,
                                    Raw = JsonConvert.SerializeObject(consumeResult)
                                });
                            }
                            catch (ConsumeException e)
                            {
                                _log.LogError(GetType(), "Error occured: {error}", e.Message);
                            }
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        // Ensure the consumer leaves the group cleanly and final offsets are committed.
                        consumer.Close();
                    }
                }
            }, cts.Token);
        }

        public void RemoveSubscription(string topic)
        {
            if (topicSubCancellationTokenSources.ContainsKey(topic))
            {
                topicSubCancellationTokenSources.TryGetValue(topic, out var cts);
                cts.Cancel();
            }
            else
            {
                _log.LogError(GetType(), "could not find subscription to topic {topic}", topic);
            }
        }

        public async Task Run()
        {
            await Task.Run(async() =>
            {
                var cts = new CancellationTokenSource();
                Console.CancelKeyPress += (_, e) =>
                {
                    e.Cancel = true; // prevent the process from terminating.
                    cts.Cancel();
                };
                while (!cts.Token.IsCancellationRequested)
                {
                    Task _currentExecution = Task.Delay(5000, cts.Token);
                    await _currentExecution.ContinueWith(task =>
                    {
                        _log.LogDebug(GetType(), "Still running");
                    });
                }
            });
        }
    }
}
