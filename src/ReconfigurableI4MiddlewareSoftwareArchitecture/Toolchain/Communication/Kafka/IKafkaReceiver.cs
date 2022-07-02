using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace I4ToolchainDotnetCore.Communication.Kafka
{
    /// <summary>
    /// Responsible for receiving messages from multiple topics, the client can subscribe to multiple topics at once, add new ones and remove them again
    /// </summary>
    public interface IKafkaReceiver
    {
        /// <summary>
        /// Responsible for initiating the consumation of messages from kafka topics, starting the primary loop
        /// </summary>
        public void Run();
        /// <summary>
        /// Responsible for adding a new subscription to be listened to
        /// </summary>
        /// <param name="topic"></param>
        public void AddSubscription(string topic);
        /// <summary>
        /// Responsible for removing a previously subscribed to topic
        /// </summary>
        /// <param name="topic"></param>
        public void RemoveSubscription(string topic);

    }
}
