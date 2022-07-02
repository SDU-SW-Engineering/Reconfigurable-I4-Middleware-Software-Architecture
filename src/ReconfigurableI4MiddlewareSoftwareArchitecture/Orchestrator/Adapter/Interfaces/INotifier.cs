using System;
using System.Collections.Generic;
using System.Text;

namespace Orchestrator.Adapter.Interfaces
{
    public interface INotifier
    {
        public void Notify(String message);
        public String GetKeyword();
    }
}
