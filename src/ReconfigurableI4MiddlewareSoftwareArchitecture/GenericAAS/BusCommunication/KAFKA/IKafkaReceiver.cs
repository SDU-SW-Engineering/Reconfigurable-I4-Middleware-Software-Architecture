using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace GenericAAS.BusCommunication.KAFKA
{
    /// <summary>
    /// Responsible for receiving messages from multiple topics, the client can subscribe to multiple topics at once, add new ones and remove them again
    /// </summary>
    public interface IKafkaReceiver
    {
        /// <summary>
        /// Responsible for initiating the consumation of messages from kafka topics, starting the primary loop
        /// </summary>
        public Task Run();
        /// <summary>
        /// Responsible for adding a new subscription to be listened to
        /// </summary>
        /// <param name="topic"></param>
        public void AddSubscription(string topic, Action<string, BusMessage> msgHandler);
        /// <summary>
        /// Responsible for removing a previously subscribed to topic
        /// </summary>
        /// <param name="topic"></param>
        public void RemoveSubscription(string topic);

    }
}
