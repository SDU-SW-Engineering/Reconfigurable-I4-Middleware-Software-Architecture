using Confluent.Kafka;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using I4ToolchainDotnetCore.Logging;

namespace Orchestrator.Adapter.Kafka
{
    public class KafkaProducer
    {
        private readonly II4Logger _log;
        private readonly string KAFKA_HOST = Environment.GetEnvironmentVariable("KAFKA_HOST") ?? "192.168.1.12:9092";
        private static KafkaProducer kafkaProducer;
        private ProducerConfig config;
        private IProducer<Null, string> producer;

        public KafkaProducer(II4Logger log)
        {
            _log = log;
            config = new ProducerConfig { BootstrapServers = KAFKA_HOST };
            producer = new ProducerBuilder<Null, string>(config).Build();
        }

        public void SendMessage(string topic, JObject message)
        {
            try
            {
                producer.Produce(topic, new Message<Null, string> { Value = message.ToString() }, handler);
            }
            catch (ProduceException<Null, string> e)
            {
                Console.WriteLine($"Delivery failed: {e.Error.Reason}");
            }
        }

        private void handler(DeliveryReport<Null, string> r)
        {
            if (!r.Error.IsError)
            {
                _log.LogDebug(GetType(), "Delivered message to kafka topic: {kafkatopic}, offset: {currentOffset}", r.Topic, r.TopicPartitionOffset);
            }
            else
            {
                _log.LogError(GetType(),$"Delivery Error: {r.Error.Reason}");
            }
        }
    }
}
