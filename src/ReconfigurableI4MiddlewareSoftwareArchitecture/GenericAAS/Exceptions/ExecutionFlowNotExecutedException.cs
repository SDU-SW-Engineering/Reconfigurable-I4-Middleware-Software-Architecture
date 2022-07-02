using System;
namespace GenericAAS.Exceptions
{
    public class ExecutionFlowNotExecutedException : Exception
    {
        public ExecutionFlowNotExecutedException()
        {
        }

        public ExecutionFlowNotExecutedException(string message)
            : base(message)
        {
        }

        public ExecutionFlowNotExecutedException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}


