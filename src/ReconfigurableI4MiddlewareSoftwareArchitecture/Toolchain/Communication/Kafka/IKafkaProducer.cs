using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace I4ToolchainDotnetCore.Communication.Kafka
{
    /// <summary>
    /// Responsible for sending messages to one or more specified topics
    /// </summary>
    public interface IKafkaProducer
    {
        void ProduceMessage(List<string> topic, JObject message);
    }
}