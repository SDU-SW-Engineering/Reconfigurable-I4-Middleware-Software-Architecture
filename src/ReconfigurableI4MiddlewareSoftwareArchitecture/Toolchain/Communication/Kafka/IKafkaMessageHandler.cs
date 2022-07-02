using System;
using System.Collections.Generic;
using System.Text;

namespace I4ToolchainDotnetCore.Communication.Kafka
{
    /// <summary>
    /// Responsible for handling all the incomming kafka messages
    /// </summary>
    public interface IKafkaMessageHandler
    {
        /// <summary>
        /// The method, which is called by the IKafkaReceiver implementation to handle a received message
        /// </summary>
        /// <param name="msg"></param>
        public void HandleMessage(KafkaMessage msg);
    }
}
