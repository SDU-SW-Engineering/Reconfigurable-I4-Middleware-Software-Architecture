using System;
namespace GenericAAS.Exceptions
{
    public class StepNotExecutedException : Exception
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


