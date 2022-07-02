using System;
using Newtonsoft.Json.Linq;

namespace GenericAAS.BusCommunication
{
    public interface IBusClient
    {
        public void Initialise();
        public void AddSubscription(string topic, Action<string, BusMessage> msgHandler);
        public void SetStatusTopic(string topic);
        public void RemoveSubscription(string topic);
        public void ProduceMessage(string topic, JObject content);
        public void ProduceMessageToStandardTopic(JObject content);
    }
}

