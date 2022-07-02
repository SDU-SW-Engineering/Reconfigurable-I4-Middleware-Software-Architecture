using System;
namespace GenericAAS.Exceptions
{
    public class ExecutionInitializationException : Exception
    {
        public ExecutionInitializationException()
        {
        }

        public ExecutionInitializationException(string message)
            : base(message)
        {
        }

        public ExecutionInitializationException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}


