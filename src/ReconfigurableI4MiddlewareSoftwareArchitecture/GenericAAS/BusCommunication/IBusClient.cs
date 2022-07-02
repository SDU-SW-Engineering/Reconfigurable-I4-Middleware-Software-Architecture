using System;
using Newtonsoft.Json.Linq;

namespace GenericAAS.BusCommunication
{
    /// <summary>
    /// Responsible for handling the interaction with the message bus.
    /// </summary>
    public interface IBusClient
    {
        /// <summary>
        /// Initializing the interaction with the message bus, i.e. creating a connection to the message bus.
        /// </summary>
        public void Initialise();
        /// <summary>
        /// Adding a subscription to a topic, including a messagehandler that is responsible for handling the
        /// messages that arrive on the topic.
        /// </summary>
        /// <param name="topic">The topic to subscribe to</param>
        /// <param name="msgHandler">The message handling function that handles the messages on the topic</param>
        public void AddSubscription(string topic, Action<string, BusMessage> msgHandler);
        /// <summary>
        /// Responsible for setting the status topic, that defines the topic the status message
        /// are sent to.
        /// </summary>
        /// <param name="topic">The status topic</param>
        public void SetStatusTopic(string topic);
        /// <summary>
        /// Responsible for removing a subscription to a topic
        /// </summary>
        /// <param name="topic">topic to remove subscription from</param>
        public void RemoveSubscription(string topic);
        /// <summary>
        /// Responsible for sending a message to a topic.
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="content"></param>
        public void ProduceMessage(string topic, JObject content);
        /// <summary>
        /// Responsible for producing a message to the status topic
        /// </summary>
        /// <param name="content"></param>
        public void ProduceMessageToStandardTopic(JObject content);
    }
}

