using I4ToolchainDotnetCore.Communication.Message;
using System;
using System.Collections.Generic;
using System.Text;

namespace Orchestrator.DomainObjects
{
    public class CapabilityRequestMessage : Message
    {
        public List<string> capabilities { get; set; }
        public CapabilityRequestMessage() : base("capability_request")
        {
        }
    }
}
