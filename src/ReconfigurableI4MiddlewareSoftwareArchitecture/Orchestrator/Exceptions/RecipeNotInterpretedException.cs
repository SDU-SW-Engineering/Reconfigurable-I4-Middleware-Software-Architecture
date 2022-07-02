using I4ToolchainDotnetCore.Logging;
using I4ToolchainDotnetCore.ServiceLayer.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Orchestrator.Exceptions
{
    class RecipeNotInterpretedException : I4Exception
    {
        public RecipeNotInterpretedException()
        {
        }

        public RecipeNotInterpretedException(string message)
            : base(message)
        {
        }

        public RecipeNotInterpretedException(string message, Exception inner)
            : base(message, inner)
        {
        }

    }
}
