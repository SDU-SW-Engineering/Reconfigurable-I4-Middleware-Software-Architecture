using I4ToolchainDotnetCore.Communication.Message;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace I4ToolchainDotnetCore.ServiceLayer.Operation
{
    public interface IOperationResponseFactory
    {
        JObject CreateResponseMessage(Message operationMessage, JObject response);
    }
}
