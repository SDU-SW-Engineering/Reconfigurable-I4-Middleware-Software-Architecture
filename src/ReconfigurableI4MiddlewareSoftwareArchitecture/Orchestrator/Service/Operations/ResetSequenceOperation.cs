using I4ToolchainDotnetCore.Logging;
using I4ToolchainDotnetCore.ServiceLayer.Operation;
using Newtonsoft.Json.Linq;
using Orchestrator.Adapter;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Orchestrator.Service.Operations
{
    class ResetSequenceOperation : IOperation
    {
        private static readonly string KEYWORD = "resetDroneFabrication";
        private readonly ICoordinator coordinator;
        private readonly II4Logger log;

        public ResetSequenceOperation(II4Logger log, ICoordinator coordinator)
        {
            this.log = log;
            this.coordinator = coordinator;

        }

        public IOperation Clone()
        {
            throw new NotImplementedException();
        }

        public string GetOperationKeyword()
        {
            return KEYWORD;
        }

        public string GetStatus()
        {
            throw new NotImplementedException();
        }


        public async Task HandleOperation(string Topic, OperationMessage msg)
        {
            DateTime start = DateTime.Now;
            log.LogInformation(GetType(), "Starting execution of recipe {operationKeyword} based on order: {order}", KEYWORD, msg.orderId);
            coordinator.StopRecipeExecution();
            log.LogInformation(GetType(), "The execution of recipe {operationKeyword} took {operationDuration} milliseconds", KEYWORD, DateTime.Now.Subtract(start).TotalMilliseconds);
        }
    }
}
