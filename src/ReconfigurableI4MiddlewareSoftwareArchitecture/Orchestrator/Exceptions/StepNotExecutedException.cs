
using I4ToolchainDotnetCore.Logging;
using I4ToolchainDotnetCore.ServiceLayer.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Orchestrator.Exceptions
{
    public class StepNotExecutedException : I4Exception
    {
        public StepNotExecutedException()
        {
        }

        public StepNotExecutedException(string message)
            : base(message)
        {
        }

        public StepNotExecutedException(string message, Exception inner)
            : base(message, inner)
        {
        }

    }
}
