using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using I4ToolchainDotnetCore.Logging;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;

namespace GenericAAS.BusCommunication.KAFKA
{
    public class KafkaClient : IBusClient
    {
        private string _host;
        private string _port;
        private string _groupId;
        private IKafkaProducer producer;
        private IKafkaReceiver receiver;
        private readonly II4Logger _log;
        private string standartTopic;

        public KafkaClient(JObject config, II4Logger log)
        {
            _host = ExtractString("host", config);
            _port = ExtractString("port", config);
            _groupId = Guid.NewGuid().ToString();
            _log = log;
            standartTopic = "DEFAULT_STATUS";
            producer = new KafkaProducer(_host, _port, log);
            receiver = new KafkaMultiThreadReceiver(_host, _port, _groupId, log, producer);
            
        }
        
        private string ExtractString(string key, JObject config)
        {
            if (config.TryGetValue(key, out JToken hostToken)) return hostToken.Value<string>();
            throw new ArgumentException($"Could not find value: {key} in configuration");
        }
        

        public void AddSubscription(string topic, Action<string, BusMessage> msgHandler)
        {
            receiver.AddSubscription(topic, msgHandler);
        }

        public void SetStatusTopic(string topic)
        {
            this.standartTopic = topic;
        }

        public void Initialise()
        {
            var cts = new CancellationTokenSource();
            
            Task task = Task.Run(async () =>
            {
                _log.LogDebug(GetType(),$"Starting kafka receiver");
                await receiver.Run();
                _log.LogDebug(GetType(),$"Stopping kafka receiver");

            }, cts.Token);
            
        }

        public void ProduceMessage(string topic, JObject content)
        {
            producer.ProduceMessage(topic, content);
        }

        public void ProduceMessageToStandardTopic(JObject content)
        {
            producer.ProduceMessage(standartTopic, content);
        }

        public void RemoveSubscription(string topic)
        {
            throw new NotImplementedException();
        }
    }
}

