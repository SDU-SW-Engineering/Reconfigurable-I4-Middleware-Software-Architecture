using System;
using Microsoft.Extensions.Configuration;

namespace I4ToolchainDotnetCore.Logging
{
    public abstract class LoggingProvider
    {
        protected readonly IConfiguration config;
        protected readonly string serviceId;

        protected LoggingProvider(IConfiguration config, string serviceId)
        {
            this.config = config;
            this.serviceId = serviceId;
        }

        public abstract void Log(LogLevel level, Type callingClass, string msg, params Object[] args);
        public abstract void Log(LogLevel level, Type callingClass, Exception e, string msg, params Object[] args);

        public abstract string GetLoggerId();
    }
}
