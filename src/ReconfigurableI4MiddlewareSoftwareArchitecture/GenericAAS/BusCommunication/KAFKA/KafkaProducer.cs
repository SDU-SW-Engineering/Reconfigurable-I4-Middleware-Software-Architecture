using Confluent.Kafka;
using I4ToolchainDotnetCore.Logging;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace GenericAAS.BusCommunication.KAFKA
{
    public class KafkaProducer : IKafkaProducer
    {
        private readonly string _host;
        private readonly string _port;
        private ProducerConfig producerConfig;
        private IProducer<Null, string> producer;
        private II4Logger _log;

        public KafkaProducer(string host, string port, II4Logger log)
        {
            _log = log;
            _host = host;
            _port = port;
            producerConfig = new ProducerConfig { BootstrapServers = $"{_host}:{_port}",
                };
            _log.LogDebug(GetType(), $"{_host}:{_port}");
            producer = new ProducerBuilder<Null, string>(producerConfig).Build();
        }

        public void ProduceMessage(string topic, JObject message)
        {
            try
            {
                producer.Produce(topic, new Message<Null, string> { Value = message.ToString() }, handler);
            }
            catch (ProduceException<Null, string> e)
            {
                _log.LogError(GetType(), "Delivery to topics {topics} failed: {error}", string.Join(", ", topic), e.Error.Reason);
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
