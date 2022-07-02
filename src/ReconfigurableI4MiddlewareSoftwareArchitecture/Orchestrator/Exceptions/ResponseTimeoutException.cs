using I4ToolchainDotnetCore.Logging;
using I4ToolchainDotnetCore.ServiceLayer.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Orchestrator.Exceptions
{
    class ResponseTimeoutException : I4Exception
    {
        public ResponseTimeoutException()
        {
        }

        public ResponseTimeoutException(string message)
            : base(message)
        {
        }

        public ResponseTimeoutException(string message, Exception inner)
            : base(message, inner)
        {
        }

    }
}
