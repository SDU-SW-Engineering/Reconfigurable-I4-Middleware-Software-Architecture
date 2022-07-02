
using I4ToolchainDotnetCore.Logging;
using I4ToolchainDotnetCore.ServiceLayer.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Orchestrator.Exceptions
{
    class OrderNotExecutedException : I4Exception
    {
        public OrderNotExecutedException()
        {
        }

        public OrderNotExecutedException(string message)
            : base(message)
        {
        }

        public OrderNotExecutedException(string message, Exception inner)
            : base(message, inner)
        {
        }

    }
}
