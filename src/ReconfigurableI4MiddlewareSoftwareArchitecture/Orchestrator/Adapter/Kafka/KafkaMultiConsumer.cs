using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using I4ToolchainDotnetCore.Logging;

namespace Orchestrator.Adapter.Kafka
{
    public class KafkaMultiConsumer : IObservable<KafkaMessage>, IKafkaMultiConsumer
    {
        private ConcurrentDictionary<string, List<string>> _topicSnapshots;
        List<IObserver<KafkaMessage>> kafkaMessageObservers;
        CancellationTokenSource cts;
        private IConfiguration _config;
        private readonly II4Logger _log;

        public KafkaMultiConsumer(IConfiguration config, II4Logger log)
        {
            _config = config;
            _log = log;
            kafkaMessageObservers = new List<IObserver<KafkaMessage>>();
            cts = new CancellationTokenSource();
        }

        public async void StartConsumption(List<String> topics)
        {
            cts = new CancellationTokenSource();
            Console.WriteLine($"Subscribing to: {string.Join(", ", new HashSet<string>(topics))}");
            var conf = new ConsumerConfig
            {
                GroupId = "OrchestrationConsumer",
                BootstrapServers = _config.GetValue<string>("KAFKA_HOST") ?? "192.168.1.12:9092",
                // Note: The AutoOffsetReset property determines the start offset in the event
                // there are not yet any committed offsets for the consumer group for the
                // topic/partitions of interest. By default, offsets are committed
                // automatically, so in this example, consumption will only start from the
                // earliest message in the topic 'my-topic' the first time you run the program.
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = false,
                EnablePartitionEof = true,
            };
            foreach (var topic in new HashSet<string>(topics))
            {

                _log.LogDebug(GetType(),"Subscribing to {topic}", topic);
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

                                    if (consumeResult.IsPartitionEOF)
                                    {
                                        Console.WriteLine(
                                            $"Reached end of topic {consumeResult.Topic}, partition {consumeResult.Partition}, offset {consumeResult.Offset}.");

                                        continue;
                                    }
                                    //Console.WriteLine($"Consumed message '{consumeResult.Value}' at: '{consumeResult.TopicPartitionOffset}'.");
                                    PublishConsumeResult(consumeResult);
                                    int commitPeriod = 1;
                                    if (consumeResult.Offset % commitPeriod == 0)
                                    {
                                        // The Commit method sends a "commit offsets" request to the Kafka
                                        // cluster and synchronously waits for the response. This is very
                                        // slow compared to the rate at which the consumer is capable of
                                        // consuming messages. A high performance application will typically
                                        // commit offsets relatively infrequently and be designed handle
                                        // duplicate messages in the event of failure.
                                        try
                                        {
                                            consumer.Commit(consumeResult);
                                        }
                                        catch (KafkaException e)
                                        {
                                            Console.WriteLine($"Commit error: {e.Error.Reason}");
                                        }
                                    }

                                }
                                catch (ConsumeException e)
                                {
                                    Console.WriteLine($"Error occured: {e.Error.Reason}");
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




        }

        public void StopConsumption()
        {
            cts.Cancel();
        }
        private void PublishConsumeResult(ConsumeResult<Ignore, string> consumeResult)
        {
            var message = new KafkaMessage() { Topic = consumeResult.Topic, Value = consumeResult.Message.Value, TimeStamp = consumeResult.Message.Timestamp.UtcDateTime.ToLocalTime() };
            foreach (var observer in kafkaMessageObservers)
            {
                observer.OnNext(message);
            }
        }

        public IDisposable Subscribe(IObserver<KafkaMessage> observer)
        {
            if (!kafkaMessageObservers.Contains(observer))
                kafkaMessageObservers.Add(observer);
            return new KafkaMessageUnsubscriber(kafkaMessageObservers, observer);
        }
    }
}
