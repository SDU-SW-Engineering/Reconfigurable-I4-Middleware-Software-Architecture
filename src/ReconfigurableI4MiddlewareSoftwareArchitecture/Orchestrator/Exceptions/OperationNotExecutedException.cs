using I4ToolchainDotnetCore.Logging;
using I4ToolchainDotnetCore.ServiceLayer.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Orchestrator.Exceptions
{
    class OperationNotExecutedException : I4Exception
    {
        public OperationNotExecutedException()
        {
        }

        public OperationNotExecutedException(string message)
            : base(message)
        {
        }

        public OperationNotExecutedException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
