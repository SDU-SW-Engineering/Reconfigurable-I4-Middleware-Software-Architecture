using I4ToolchainDotnetCore.Communication.Message;
using System;
using System.Collections.Generic;
using System.Text;

namespace Orchestrator.DomainObjects
{
    public class CapabilityResponseMessage : Message
    {
        public List<string> capabilities { get; set; }
        public CapabilityResponseMessage() : base("capability_response")
        {
        }
    }
}
