using I4ToolchainDotnetCore.Logging;
using I4ToolchainDotnetCore.ServiceLayer.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Orchestrator.Exceptions
{
    class RecipeNotFoundException : I4Exception
    {
        public RecipeNotFoundException()
        {
        }

        public RecipeNotFoundException(string message)
            : base(message)
        {
        }

        public RecipeNotFoundException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
