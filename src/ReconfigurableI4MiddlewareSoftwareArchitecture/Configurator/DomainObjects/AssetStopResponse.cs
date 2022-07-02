using I4ToolchainDotnetCore.Communication.Message;
using System;
using System.Collections.Generic;
using System.Text;

namespace Configurator.DomainObjects
{
    public class AssetStopResponse : Message
    {
        public AssetStopResponse() : base("stop")
        {

        }
    }
}
