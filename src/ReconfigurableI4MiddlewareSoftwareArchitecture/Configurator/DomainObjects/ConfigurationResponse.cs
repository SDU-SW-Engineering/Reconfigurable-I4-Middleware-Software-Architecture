using I4ToolchainDotnetCore.Communication.Message;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace Configurator.DomainObjects
{
    public class ConfigurationResponse : Message
    {
        public JObject recipe { get; set; }
        public ConfigurationResponse() : base(MSG_TYPE.CONFIGURATION_RESPONSE.ToString())
        {

        }
    }
}
