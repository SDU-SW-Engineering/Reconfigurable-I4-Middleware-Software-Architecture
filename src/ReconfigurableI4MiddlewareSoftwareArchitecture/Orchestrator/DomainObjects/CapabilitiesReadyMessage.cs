using I4ToolchainDotnetCore.Communication.Message;
using System;
using System.Collections.Generic;
using System.Text;

namespace Orchestrator.DomainObjects
{
    public class CapabilitiesReadyMessage : Message
    {
        public CapabilitiesReadyMessage() : base("capabilities_ready")
        {
        }
    }
}
