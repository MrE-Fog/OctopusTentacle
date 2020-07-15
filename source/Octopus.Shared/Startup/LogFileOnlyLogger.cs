using System;
using System.IO;
using System.Linq;
using System.Reflection;
using NLog;
using Octopus.Diagnostics;
using Octopus.Shared.Diagnostics;
using Octopus.Shared.Util;

namespace Octopus.Shared.Startup
{
    public interface ILogFileOnlyLogger
    {
        void Info(string message);
        void Warn(string message);
        void Error(string message);
        void Error(Exception ex, string message);
        void AddSensitiveValues(string?[] values);
    }

    /// <summary>
    /// This logger has a special rule in the nlog config file to write directly to the log file, and prevent logging to the Console.
    /// </summary>
    public class LogFileOnlyLogger : ILogFileOnlyLogger
    {
        static readonly string LoggerName = nameof(LogFileOnlyLogger);
        static readonly ILogger Log = LogManager.GetLogger(LoggerName);

        private static readonly string EntryExecutable = PlatformDetection.IsRunningOnWindows
            ? Path.GetFileName(Assembly.GetEntryAssembly()?.FullProcessPath())
            : $"{Path.GetFileName(Assembly.GetEntryAssembly()?.FullProcessPath())}.exe";
        static readonly string HelpMessage = $"The {EntryExecutable}.nlog file should have a rule matching the name {LoggerName} where log messages are restricted to the log file, never written to stdout or stderr.";

        public static void AssertConfigurationIsCorrect()
        {
            var rule = LogManager.Configuration.LoggingRules.SingleOrDefault(r => r.LoggerNamePattern == LoggerName);
            if (rule == null)
                throw new Exception($"It looks like the {LoggerName} logging rule is not configured. {HelpMessage}");

            if (rule.Targets.Count != 1)
                throw new Exception($"The {LoggerName} rule should only have a single target. {HelpMessage}");

            var fileTarget = rule.Targets.Single() as NLog.Targets.FileTarget;
            if (fileTarget == null)
                throw new Exception($"The {LoggerName} rule should write to a file target. {HelpMessage}");
        }

        public ILogContext Context { get; set; } = new LogContext();
        public static ILogFileOnlyLogger Current { get; }= new LogFileOnlyLogger();

        public void Info(string message) => Context.SafeSanitize(message, s => Log.Info(s));
        public void Warn(string message) => Context.SafeSanitize(message, s => Log.Warn(s));
        public void Error(string message) => Context.SafeSanitize(message, s => Log.Error(s));
        public void Error(Exception ex, string message) => Context.SafeSanitize(message, s => Log.Error(ex, s));

        public void AddSensitiveValues(string?[] values)
        {
            Context = Context.WithSensitiveValues(values);
        }
    }
}
