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
    /// An Implementation of a kafka receiver, that creates a new consumer for each subscription. Good for handling multiple topics, where one or more
    /// receive a continuous stream of messages - not so good, if a lot of different topics are subscribed to.
    /// </summary>
    public class KafkaMultiThreadReceiver : IKafkaReceiver
    {
        private readonly IConfiguration _config;
        private readonly IKafkaMessageHandler _msgHandler;
        private Dictionary<string, CancellationTokenSource> topicSubCancellationTokenSources;
        private string KAFKA_GROUP_ID = "KAFKA_RECEIVER";
        private II4Logger _log;
        public KafkaMultiThreadReceiver(IConfiguration config, IKafkaMessageHandler msgHandler, II4Logger log)
        {
            _log = log;
            _config = config;
            _msgHandler = msgHandler;
            KAFKA_GROUP_ID = _config.GetValue<string>("KAFKA_GROUP_ID");
            topicSubCancellationTokenSources = new Dictionary<string, CancellationTokenSource>();
        }
        public void AddSubscription(string topic)
        {
            var conf = new ConsumerConfig
            {
                GroupId = KAFKA_GROUP_ID,
                BootstrapServers = _config.GetValue<string>("KAFKA_HOST") ?? "192.168.0.105:9092",
                AutoOffsetReset = AutoOffsetReset.Earliest,
            };
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
                                var msg = new KafkaMessage()
                                {
                                    Topic = consumeResult.Topic,
                                    Message = consumeResult.Message.Value,
                                    Timestamp = consumeResult.Message.Timestamp.UtcDateTime,
                                    Raw = JsonConvert.SerializeObject(consumeResult)
                                };
                                _msgHandler.HandleMessage(msg);
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

        public void Run()
        {
            Task task = Task.Run(async() =>
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
            task.GetAwaiter().GetResult();
        }
    }
}
