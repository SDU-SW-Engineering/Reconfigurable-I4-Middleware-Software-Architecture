using I4ToolchainDotnetCore.Communication.Message;
using System;
using System.Collections.Generic;
using System.Text;

namespace Configurator.DomainObjects
{
    public class ConfigurationRequest : Message
    {
        public List<string> capabilities { get; set; }
        public List<string> notWorkingService { get; set; }
        public ConfigurationRequest() : base(MSG_TYPE.CONFIGURATION_REQUEST.ToString())
        {

        }
    }
}
