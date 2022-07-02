using System.Collections.Generic;

namespace I4ToolchainDotnetCore.ServiceLayer.Operation
{
    public interface IOperationManager
    {
        void StartOperation(string topic, OperationMessage msg);
        void ClearQueue();
        List<IOperation> GetOperations();
        Queue<OperationExecutionRequest> GetQueue();
        IOperation GetExecutingOperation();
    }
}