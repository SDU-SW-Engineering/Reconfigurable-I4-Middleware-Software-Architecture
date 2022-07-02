using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using I4ToolchainDotnetCore.Logging;
using I4ToolchainDotnetCore.ServiceLayer.Exceptions;
using Microsoft.Extensions.Configuration;

namespace I4ToolchainDotnetCore.ServiceLayer.Operation
{
    public class OperationManager : IOperationManager
    {
        private readonly List<IOperation> operations;
        private readonly II4Logger log;
        private Queue<OperationExecutionRequest> operationsQueue;
        private bool currentlyExecuting = false;
        private IOperation executingOperation = null;

        public OperationManager(IConfiguration config, IEnumerable<IOperation> operations, II4Logger log)
        {
            this.operations = operations.ToList();
            operationsQueue = new Queue<OperationExecutionRequest>();
            this.log = log;
        }

        public void StartOperation(String topic, OperationMessage message)
        {
            string operationKeyWord = message.operation;
            if (!OperationExists(operationKeyWord)) throw new ArgumentException($"The operation {operationKeyWord} does not exist");
            try
            {
                log.LogDebug(GetType(), "Starting operation: " + message.operation);
                var operation = operations.Find(o => o.GetOperationKeyword() == operationKeyWord).Clone();
                operationsQueue.Enqueue(new OperationExecutionRequest() { Topic = topic, Message = message, Operation = operation});
                log.LogDebug(GetType(), "current queue length: {queue}", operationsQueue.Count);
                if (!currentlyExecuting)
                {
                    StartExecution();
                }
            } catch (ArgumentNullException ex)
            {
                log.LogError(GetType(), "could not find operation: {operation}", message.operation);
            }
            
            log.LogDebug(GetType(), "The operation with the keyword {keyword} was handled successfully", operationKeyWord);
        }

        private async Task StartExecution()
        {
            while (operationsQueue.Count > 0)
            {
                currentlyExecuting = true;
                var operationReq = operationsQueue.Dequeue();
                try
                {
                    executingOperation = operationReq.Operation;
                    await executingOperation.HandleOperation(operationReq.Topic, operationReq.Message);
                }
                catch (I4Exception e)
                {
                    log.LogError(GetType(), e, "The operation with the keyword {operationKeyword} returned an I4Exception with message: {ExceptionMessage}", operationReq.Operation.GetOperationKeyword(), e.Message);
                }
                catch (Exception e)
                {
                    log.LogError(GetType(), e, "The operation with the keyword {operationKeyword} returned an Exception with message: {ExceptionMessage}", operationReq.Operation.GetOperationKeyword(), e.Message);
                }
                finally
                {
                    executingOperation = null;
                }
                
            }
            currentlyExecuting = false;
            
        }

        private bool OperationExists(string operation)
        {
            bool operationExists = operations.Exists(x => x.GetOperationKeyword().Equals(operation));
            log.LogDebug(GetType(), "The Operation with keyword {operationKeyword} exists: {operationExists}", operation, operationExists);
            return operationExists;
        }
        public List<IOperation> GetOperations()
        {
            return operations;
        }

        public void ClearQueue()
        {
            operationsQueue.Clear();
        }

        public Queue<OperationExecutionRequest> GetQueue()
        {
            return operationsQueue;
        }

        public IOperation GetExecutingOperation()
        {
            return executingOperation;
        }
    }
}
