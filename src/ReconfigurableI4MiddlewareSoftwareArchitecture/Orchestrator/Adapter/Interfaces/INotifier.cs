using System;
using System.Collections.Generic;
using System.Text;

namespace Orchestrator.Adapter.Interfaces
{
    /// <summary>
    /// Not Used
    /// </summary>
    public interface INotifier
    {
        public void Notify(String message);
        public String GetKeyword();
    }
}
