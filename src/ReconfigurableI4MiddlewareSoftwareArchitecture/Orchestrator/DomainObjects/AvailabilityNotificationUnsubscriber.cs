using System;
using System.Collections.Generic;
using System.Text;
using Orchestrator.DomainObjects;

namespace Orchestrator.DomainObjects
{
    public class AvailabilityNotificationUnsubscriber : IDisposable
    {
        private List<IObserver<AvailabilityNotification>> _observers;
        private IObserver<AvailabilityNotification> _observer;

        public AvailabilityNotificationUnsubscriber(List<IObserver<AvailabilityNotification>> observers, IObserver<AvailabilityNotification> observer)
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
