using System;
namespace Configurator.Exceptions
{
    public class ContainerHandlingException : Exception
    {
        public ContainerHandlingException()
        {
        }

        public ContainerHandlingException(string message)
            : base(message)
        {
        }

        public ContainerHandlingException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}


