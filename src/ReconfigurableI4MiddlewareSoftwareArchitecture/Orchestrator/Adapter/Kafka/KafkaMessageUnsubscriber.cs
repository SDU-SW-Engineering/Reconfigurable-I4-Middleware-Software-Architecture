using System;
using System.Collections.Generic;
using System.Text;

namespace Orchestrator.Adapter.Kafka
{
    public class KafkaMessageUnsubscriber : IDisposable
    {
        private List<IObserver<KafkaMessage>> _observers;
        private IObserver<KafkaMessage> _observer;

        public KafkaMessageUnsubscriber(List<IObserver<KafkaMessage>> observers, IObserver<KafkaMessage> observer)
        {
            this._observers = observers;
            this._observer = observer;
        }

        public void Dispose()
        {
            if (!(_observer == null)) _observers.Remove(_observer);
        }
    }
}
