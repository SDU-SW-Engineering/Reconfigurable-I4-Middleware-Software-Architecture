using Orchestrator.DomainObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace Orchestrator.Service
{
    public interface IConfigurationHandler
    {
        void HandleConfigurationRequest(CapabilityRequestMessage msg);
    }
}
