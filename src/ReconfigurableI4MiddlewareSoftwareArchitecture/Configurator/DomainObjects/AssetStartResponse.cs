using I4ToolchainDotnetCore.Communication.Message;
using System;
using System.Collections.Generic;
using System.Text;

namespace Configurator.DomainObjects
{
    public class AssetStartResponse : Message
    {
        public AssetStartResponse() : base("initial_heartbeat")
        {

        }
    }
}
