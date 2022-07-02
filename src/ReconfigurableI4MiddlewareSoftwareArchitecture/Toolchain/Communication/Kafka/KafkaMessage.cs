using System;
using System.Collections.Generic;
using System.Text;

namespace I4ToolchainDotnetCore.Communication.Kafka
{
    public class KafkaMessage
    {
        public string Topic { get; set; }
        public string Message { get; set; }
        public DateTime Timestamp { get; set; }
        public string Raw { get; set; }

        public override string ToString()
        {
            return $"Message from topic: {Topic}, with content: {Message}, and timestamp {Timestamp}. \nRaw: {Raw}";
        }
    }
}
