using System;

namespace I4ToolchainDotnetCore.Logging
{
    public interface II4Logger
    {
        void LogDebug(Type callingClass, Exception e, string msg, params object[] args);
        void LogDebug(Type callingClass, string msg, params object[] args);
        void LogError(Type callingClass, Exception e, string msg, params object[] args);
        void LogError(Type callingClass, string msg, params object[] args);
        void LogFatal(Type callingClass, Exception e, string msg, params object[] args);
        void LogFatal(Type callingClass, string msg, params object[] args);
        void LogInformation(Type callingClass, Exception e, string msg, params object[] args);
        void LogInformation(Type callingClass, string msg, params object[] args);
        void LogVerbose(Type callingClass, Exception e, string msg, params object[] args);
        void LogVerbose(Type callingClass, string msg, params object[] args);
        void LogWarning(Type callingClass, Exception e, string msg, params object[] args);
        void LogWarning(Type callingClass, string msg, params object[] args);
    }
}