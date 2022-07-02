using I4ToolchainDotnetCore.Logging;
using I4ToolchainDotnetCore.ServiceLayer.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Orchestrator.Exceptions
{
    public class CommandNotExecutedSuccessfullyException : I4Exception
    {
        public CommandNotExecutedSuccessfullyException()
        {
        }

        public CommandNotExecutedSuccessfullyException(string message)
            : base(message)
        {
        }

        public CommandNotExecutedSuccessfullyException(string message, Exception inner)
            : base(message, inner)
        {
        }

    }
}
