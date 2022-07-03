using Orchestrator.DomainObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace Orchestrator.Service
{
    /// <summary>
    /// Not used
    /// </summary>
    public interface IConfigurationHandler
    {
        
        void HandleConfigurationRequest(CapabilityRequestMessage msg);
    }
}
