using System;

namespace I4ToolchainDotnetCore.ServiceLayer.Exceptions
{
    class OperationNotExecutedException : I4Exception
    {
        private readonly string exceptionOriginOperation;
        private readonly bool executeCancelationMethod;

        public OperationNotExecutedException(String exceptionOriginOperation, bool executeCancelationMethod)
        {
            this.exceptionOriginOperation = exceptionOriginOperation;
            this.executeCancelationMethod = executeCancelationMethod;
        }
    }
}
