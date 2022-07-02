using System;
namespace GenericAAS.Exceptions
{
    public class AssetNotConnectedException : Exception
    {
        public AssetNotConnectedException()
        {
        }

        public AssetNotConnectedException(string message)
            : base(message)
        {
        }

        public AssetNotConnectedException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}


