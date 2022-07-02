using Confluent.Kafka;
using I4ToolchainDotnetCore.Logging;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace I4ToolchainDotnetCore.Communication.Kafka
{
    public class KafkaProducer : IKafkaProducer
    {
        private readonly string KAFKA_HOST;
        private ProducerConfig producerConfig;
        private IProducer<Null, string> producer;
        private IConfiguration _config;
        private II4Logger _log;

        public KafkaProducer(IConfiguration config, II4Logger log)
        {
            _log = log;
            _config = config;
            KAFKA_HOST = _config.GetValue<string>("KAFKA_HOST") ?? "192.168.1.12:9092";
            producerConfig = new ProducerConfig { BootstrapServers = KAFKA_HOST };
            producer = new ProducerBuilder<Null, string>(producerConfig).Build();
        }

        public void ProduceMessage(List<string> topics, JObject message)
        {
            try
            {
                foreach(var topic in topics)
                {
                    producer.Produce(topic, new Message<Null, string> { Value = message.ToString() }, handler);
                }
            }
            catch (ProduceException<Null, string> e)
            {
                _log.LogError(GetType(), "Delivery to topics {topics} failed: {error}", string.Join(", ", topics), e.Error.Reason);
            }
        }

        private void handler(DeliveryReport<Null, string> r)
        {
            if (!r.Error.IsError)
            {
                _log.LogDebug(GetType(), "Delivered message to {topic}", r.TopicPartitionOffset);
            }
            else
            {
                _log.LogError(GetType(), "Delivery Error: {error}", r.Error.Reason);
            }
        }

    }
}
