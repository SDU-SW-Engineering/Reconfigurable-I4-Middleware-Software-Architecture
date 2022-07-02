using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace I4ToolchainDotnetCore.Logging
{
    public class I4Logger : II4Logger
    {
        private const string VERBOSE = "verbose";
        private const string DEBUG = "debug";
        private const string INFORMATION = "information";
        private const string WARNING = "warning";
        private const string ERROR = "error";
        private const string FATAL = "fatal";

        private readonly IConfiguration config;
        Dictionary<String, List<LoggingProvider>> loggers;
        private List<LoggingProvider> loggingProviders;
        private readonly string serviceId;

        public I4Logger(IConfiguration config, String serviceId)
        {
            this.serviceId = serviceId;
            this.config = config;
            InitializeLoggerDictionary();
            loggingProviders = GetLoggingProviders();
            PopulateLoggerList();

        }

        private void InitializeLoggerDictionary()
        {
            loggers = new Dictionary<string, List<LoggingProvider>>
            {
                {VERBOSE, new List<LoggingProvider>()},
                {DEBUG, new List<LoggingProvider>()},
                {INFORMATION, new List<LoggingProvider>()},
                {WARNING, new List<LoggingProvider>()},
                {ERROR, new List<LoggingProvider>()},
                {FATAL, new List<LoggingProvider>()},
            };
        }

        private void PopulateLoggerList()
        {
            var logLevels = new Dictionary<String, List<String>>();
            config.GetSection("loggingProviders").Bind(logLevels);
            foreach (String logLevel in logLevels.Keys)
            {
                var loggerIds = new List<String>();
                config.GetSection("loggingProviders:" + logLevel).Bind(loggerIds);
                foreach (string LoggerId in loggerIds)
                {
                    addToLoglevelAndAbove(LogLevel.getLogLevel(logLevel), loggingProviders.Where(x => x.GetLoggerId().Equals(LoggerId)).ToList());
                }
            }
        }

        private void addToLoglevelAndAbove(LogLevel level, List<LoggingProvider> log)
        {
            foreach (string logLevelId in loggers.Keys)
            {
                if (LogLevel.getLogLevel(logLevelId).Value >= level.Value)
                {
                    loggers[logLevelId].AddRange(log);
                }
            }
        }
        //makes logging providers dynamically based on implementation of LoggingProvider
        private List<LoggingProvider> GetLoggingProviders()
        {
            return AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes())
                  .Where(x => typeof(LoggingProvider).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract).ToList()
                  .Select(t => Activator.CreateInstance(t, config, serviceId))
                  .Cast<LoggingProvider>().ToList();
        }

        public void LogVerbose(Type callingClass, string msg, params Object[] args)
        {
            loggers[LogLevel.Verbose.Name].ForEach(logger => logger.Log(LogLevel.Verbose, callingClass, msg, args));
        }
        public void LogVerbose(Type callingClass, Exception e, string msg, params Object[] args)
        {
            loggers[LogLevel.Verbose.Name].ForEach(logger => logger.Log(LogLevel.Verbose, callingClass, e, msg, args));
        }

        public void LogDebug(Type callingClass, string msg, params Object[] args)
        {
            loggers[LogLevel.Debug.Name].ForEach(logger => logger.Log(LogLevel.Debug, callingClass, msg, args));
        }
        public void LogDebug(Type callingClass, Exception e, string msg, params Object[] args)
        {
            loggers[LogLevel.Debug.Name].ForEach(logger => logger.Log(LogLevel.Debug, callingClass, e, msg, args));
        }

        public void LogInformation(Type callingClass, string msg, params Object[] args)
        {
            loggers[LogLevel.Information.Name].ForEach(logger => logger.Log(LogLevel.Information, callingClass, msg, args));
        }
        public void LogInformation(Type callingClass, Exception e, string msg, params Object[] args)
        {
            loggers[LogLevel.Information.Name].ForEach(logger => logger.Log(LogLevel.Information, callingClass, e, msg, args));
        }

        public void LogWarning(Type callingClass, string msg, params Object[] args)
        {
            loggers[LogLevel.Warning.Name].ForEach(logger => logger.Log(LogLevel.Warning, callingClass, msg, args));
        }
        public void LogWarning(Type callingClass, Exception e, string msg, params Object[] args)
        {
            loggers[LogLevel.Warning.Name].ForEach(logger => logger.Log(LogLevel.Warning, callingClass, e, msg, args));
        }

        public void LogError(Type callingClass, string msg, params Object[] args)
        {
            loggers[LogLevel.Error.Name].ForEach(logger => logger.Log(LogLevel.Error, callingClass, msg, args));
        }
        public void LogError(Type callingClass, Exception e, string msg, params Object[] args)
        {
            loggers[LogLevel.Error.Name].ForEach(logger => logger.Log(LogLevel.Error, callingClass, e, msg, args));
        }

        public void LogFatal(Type callingClass, string msg, params Object[] args)
        {
            loggers[LogLevel.Fatal.Name].ForEach(logger => logger.Log(LogLevel.Fatal, callingClass, msg, args));
        }
        public void LogFatal(Type callingClass, Exception e, string msg, params Object[] args)
        {
            loggers[LogLevel.Fatal.Name].ForEach(logger => logger.Log(LogLevel.Fatal, callingClass, e, msg, args));
        }

    }


}
