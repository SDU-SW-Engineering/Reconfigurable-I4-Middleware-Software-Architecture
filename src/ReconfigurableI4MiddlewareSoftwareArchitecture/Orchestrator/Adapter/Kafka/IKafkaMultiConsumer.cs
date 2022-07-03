using System;
using System.Collections.Generic;

namespace Orchestrator.Adapter.Kafka
{
    /// <summary>
    /// Responsible for the handling the subscription to multiple kafka topics
    /// </summary>
    public interface IKafkaMultiConsumer 
    {
        /// <summary>
        /// Responsible for starting subscription to a list of topics
        /// </summary>
        /// <param name="topics"></param>
        void StartConsumption(List<string> topics);
        /// <summary>
        /// Responsible for unsubscribing to all topics
        /// </summary>
        void StopConsumption();
        /// <summary>
        /// Responsible for Subscribing to a topic based on the IObservable pattern implemented in C#
        /// </summary>
        /// <param name="observer"></param>
        /// <returns></returns>
        IDisposable Subscribe(IObserver<KafkaMessage> observer);
    }
}