using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace I4ToolchainDotnetCore.ServiceLayer.Operation
{
    public interface IOperation
    {
        public IOperation Clone();
        public Task HandleOperation(string Topic, OperationMessage msg);
        public string GetOperationKeyword();
        public string GetStatus();
    }
}
