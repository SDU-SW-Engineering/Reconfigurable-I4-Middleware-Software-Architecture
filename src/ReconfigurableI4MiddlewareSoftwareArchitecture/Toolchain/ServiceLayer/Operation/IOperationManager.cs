using System.Collections.Generic;

namespace I4ToolchainDotnetCore.ServiceLayer.Operation
{
    /// <summary>
    /// Responsible for managing an incoming operation request, i.e. finding the right IOperation implementation
    /// matching the requested operation and initializing the execution.
    /// </summary>
    public interface IOperationManager
    {
        /// <summary>
        /// Responsible for starting the operation based on the origin topic and received operation message
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="msg"></param>
        void StartOperation(string topic, OperationMessage msg);
        /// <summary>
        /// Responsible for clearing the queue of operations to be executed
        /// </summary>
        void ClearQueue();
        /// <summary>
        /// Responsible for returning all available operations
        /// </summary>
        /// <returns></returns>
        List<IOperation> GetOperations();
        /// <summary>
        /// Responsible for returning the current queue of operations to be executed.
        /// </summary>
        /// <returns></returns>
        Queue<OperationExecutionRequest> GetQueue();
        /// <summary>
        /// Responsible for returning the currently executed operation.
        /// </summary>
        /// <returns></returns>
        IOperation GetExecutingOperation();
    }
}