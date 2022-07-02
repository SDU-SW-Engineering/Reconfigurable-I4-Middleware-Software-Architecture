using I4ToolchainDotnetCore.Logging;
using I4ToolchainDotnetCore.ServiceLayer.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Orchestrator.Exceptions
{
    public class RecipeNotExecutedException : I4Exception
    {
        public RecipeNotExecutedException()
        {
        }

        public RecipeNotExecutedException(string message)
            : base(message)
        {
        }

        public RecipeNotExecutedException(string message, Exception inner)
            : base(message, inner)
        {
        }

    }
}
