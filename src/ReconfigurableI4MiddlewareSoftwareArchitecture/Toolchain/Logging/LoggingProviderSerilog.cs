using System;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting.Elasticsearch;
using Serilog.Sinks.Elasticsearch;

namespace I4ToolchainDotnetCore.Logging
{
    public class LoggingProviderSerilog : LoggingProvider
    {
        private Logger log;
        private string KEYWORD = "serilog";
        public LoggingProviderSerilog(IConfiguration config, string serviceId) : base (config, serviceId)
        {
            this.setupSerilog(config);
        }

        private void setupSerilog(IConfiguration config)
        {
            var ELASTIC_HOST = config.GetValue<string>("ELASTIC_HOST") ?? "";
            var ELASTIC_PORT = config.GetValue<string>("ELASTIC_PORT") ?? "9200";
            if(ELASTIC_HOST.Length > 0)
            {
                log = new LoggerConfiguration()
                .ReadFrom.Configuration(config).WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri($"http://{ELASTIC_HOST}:{ELASTIC_PORT}")))
                .CreateLogger();
            }
            else
            {
                log = new LoggerConfiguration()
                .ReadFrom.Configuration(config).CreateLogger();
            }
            
        }

        public override string GetLoggerId()
        {
            return KEYWORD;
        }

        public override void Log(LogLevel level, Type callingClass, string msg, params Object[] args)
        {
            log.ForContext("CallingClass", callingClass.Name).ForContext("ServiceId", serviceId).Write(ConvertValueToLogEventLevel(level.Value), msg, args);
        }
        public override void Log(LogLevel level, Type callingClass, Exception e, string msg, params Object[] args)
        {
            log.ForContext("CallingClass", callingClass.Name).ForContext("ServiceId", serviceId).Write(ConvertValueToLogEventLevel(level.Value),e, msg, args);
        }

        private LogEventLevel ConvertValueToLogEventLevel(int value)
        {
            switch (value)
            {
                case 0:
                    return LogEventLevel.Verbose;
                case 1:
                    return LogEventLevel.Debug;
                case 2:
                    return LogEventLevel.Information;
                case 3:
                    return LogEventLevel.Warning;
                case 4:
                    return LogEventLevel.Error;
                case 5:
                    return LogEventLevel.Fatal;
                default:
                    return LogEventLevel.Verbose;
            }
        }
    }
}
