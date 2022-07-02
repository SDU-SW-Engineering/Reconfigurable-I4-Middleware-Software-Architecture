using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace I4ToolchainDotnetCore.ServiceLayer.Operation
{
    /// <summary>
    /// Responsible for executing an operation (Only used by the Orchestrator)
    /// </summary>
    public interface IOperation
    {
        /// <summary>
        /// Responsible for creating a clone of the IOperation
        /// </summary>
        /// <returns></returns>
        public IOperation Clone();
        /// <summary>
        /// Responsible for handling the execution of an operation detailed in the received message
        /// </summary>
        /// <param name="Topic">the origin topic, to know where to respond to</param>
        /// <param name="msg">the received operation message to handle</param>
        /// <returns></returns>
        public Task HandleOperation(string Topic, OperationMessage msg);
        /// <summary>
        /// Returning the keyword that identifies this operation
        /// </summary>
        /// <returns></returns>
        public string GetOperationKeyword();
        public string GetStatus();
    }
}
