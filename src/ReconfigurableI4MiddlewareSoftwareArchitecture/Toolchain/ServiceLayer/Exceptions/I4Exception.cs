using System;

namespace I4ToolchainDotnetCore.ServiceLayer.Exceptions
{
    public abstract class I4Exception : Exception
    {
        public I4Exception()
        {
        }

        public I4Exception(string message)
            : base(message)
        {
        }

        public I4Exception(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
