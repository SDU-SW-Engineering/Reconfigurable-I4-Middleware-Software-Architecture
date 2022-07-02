using System;

namespace I4ToolchainDotnetCore.ServiceLayer.Exceptions
{
    public class NonI4Exception : I4Exception
    {
        private Exception exception;
        public NonI4Exception(Exception e)
        {
            this.exception = e;
        }
    }
}
