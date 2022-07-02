using I4ToolchainDotnetCore.Logging;
using I4ToolchainDotnetCore.ServiceLayer.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Orchestrator.Exceptions
{
    class ExpectedParameterNotFoundException : I4Exception
    {
        public ExpectedParameterNotFoundException()
        {
        }

        public ExpectedParameterNotFoundException(string message)
            : base(message)
        {
        }

        public ExpectedParameterNotFoundException(string message, Exception inner)
            : base(message, inner)
        {
        }

    }
}
