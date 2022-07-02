using System;
using System.Collections.Generic;
using System.Text;

namespace I4ToolchainDotnetCore.ServiceLayer.Operation
{
    public class OperationExecutionRequest
    {
        public IOperation Operation { get; set; }
        public string Topic { get; set; }
        public OperationMessage Message { get; set; }
    }
}
