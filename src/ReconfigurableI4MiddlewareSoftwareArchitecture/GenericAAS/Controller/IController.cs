using System;
using GenericAAS.BusCommunication;

namespace GenericAAS.Controller
{
    public interface IController
    {
        public void Initialize();
        /// <summary>
        /// Responsible for handling the messages coming from the bus.
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="msg"></param>
        public void HandleBusMessage(string topic, BusMessage msg);
    }
}

