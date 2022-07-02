using I4ToolchainDotnetCore.Communication.Message;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace I4ToolchainDotnetCore.ServiceLayer.Operation
{
    public class OperationResponseFactory : IOperationResponseFactory
    {
        private IConfiguration _config;
        private string aasOriginId;
        public OperationResponseFactory(IConfiguration config)
        {
            _config = config;
            aasOriginId = _config.GetValue<string>("AAS_ORIGIN_ID");
        }
        public JObject CreateResponseMessage(Message operationMessage, JObject response)
        {
            JObject returnValue = new JObject();
            returnValue.Add("@id", Guid.NewGuid().ToString());
            returnValue.Add("@type", "response");
            returnValue.Add("operationId", operationMessage.id);
            returnValue.Add("aasOriginId", aasOriginId);
            returnValue.Add("aasTargetId", operationMessage.aasOriginId);
            returnValue.Add("response", response);
            //returnValue.Add("createdAt", DateTime.Now.ToLocalTime().ToString("yyyy-MM-ddTHH:mm:ss.ffffK"));
            return returnValue;
        }
    }
}
