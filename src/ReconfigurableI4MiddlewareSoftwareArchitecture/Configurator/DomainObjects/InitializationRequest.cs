using I4ToolchainDotnetCore.Communication.Message;
using System;
using System.Collections.Generic;
using System.Text;

namespace Configurator.DomainObjects
{
    public class InitializationRequest : Message
    {
        public List<string> capabilities { get; set; }
        public List<string> notWorkingService { get; set; }
        public InitializationRequest() : base("initialization_request")
        {

        }
    }
}
