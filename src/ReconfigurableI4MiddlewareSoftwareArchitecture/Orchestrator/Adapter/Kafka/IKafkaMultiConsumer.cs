using System;
using System.Collections.Generic;

namespace Orchestrator.Adapter.Kafka
{
    public interface IKafkaMultiConsumer 
    {
        void StartConsumption(List<string> topics);
        void StopConsumption();
        IDisposable Subscribe(IObserver<KafkaMessage> observer);
    }
}