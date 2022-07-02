using System;

namespace I4ToolchainDotnetCore.Logging
{
    public class LogLevel
    {
        private LogLevel(string name, int value) { Name = name; Value = value; }

        public string Name { get; set; }
        public int Value { get; set; }

        public static LogLevel Verbose { get { return new LogLevel("verbose", 0); } }
        public static LogLevel Debug { get { return new LogLevel("debug", 1); } }
        public static LogLevel Information { get { return new LogLevel("information", 2); } }
        public static LogLevel Warning { get { return new LogLevel("warning", 3); } }
        public static LogLevel Error { get { return new LogLevel("error", 4); } }
        public static LogLevel Fatal { get { return new LogLevel("fatal", 5); } }

        public static LogLevel getLogLevel(String logLevel)
        {
            switch (String.Format(logLevel).ToLower())
            {
                case "verbose":
                    return Verbose;
                case "debug":
                    return Debug;
                case "information":
                    return Information;
                case "warning":
                    return Warning;
                case "error":
                    return Error;
                case "fatal":
                    return Fatal;
                default:
                    return null;
                    throw new ArgumentException("could not find a suitable log level");
            }
        }
    }
}
