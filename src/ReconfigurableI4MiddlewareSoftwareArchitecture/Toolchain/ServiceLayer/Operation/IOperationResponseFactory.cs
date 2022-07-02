using I4ToolchainDotnetCore.Communication.Message;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace I4ToolchainDotnetCore.ServiceLayer.Operation
{
    public interface IOperationResponseFactory
    {
        /// <summary>
        /// Responsible for creating a response based on the received message, i.e. retrieving IDs from the received message
        /// and using them in the response to keep track of request response relationships
        /// </summary>
        /// <param name="operationMessage"></param>
        /// <param name="response"></param>
        /// <returns></returns>
        JObject CreateResponseMessage(Message operationMessage, JObject response);
    }
}
