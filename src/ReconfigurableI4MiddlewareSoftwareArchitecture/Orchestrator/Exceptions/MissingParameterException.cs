using I4ToolchainDotnetCore.ServiceLayer.Exceptions;
using System;

namespace Orchestrator.Exceptions
{
    class MissingParameterException : I4Exception
    {
        public MissingParameterException()
        {
        }

        public MissingParameterException(string message)
            : base(message)
        {
        }

        public MissingParameterException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
