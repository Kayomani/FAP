using System.Diagnostics;
using FAP.Domain.Entities;
using Fap.Foundation;
using NLog;
using NLog.Config;
using NLog.Targets;
using NLog.Targets.Wrappers;

namespace FAP.Domain.Services
{
    /// <summary>
    /// Trace - very detailed logs, which may include high-volume information such as protocol payloads. This log level is typically only enabled during development
    /// Debug - debugging information, less detailed than trace, typically not enabled in production environment.
    /// Info - information messages, which are normally enabled in production environment
    /// Warn - warning messages, typically for non-critical issues, which can be recovered or which are temporary failures
    /// Error - error messages
    /// Fatal - very serious errors 
    /// </summary>
    public class LogService
    {
        private readonly LoggingRule rule;
        private readonly LogServiceTarget target;

        public LogService(Model m)
        {
            LoggingConfiguration config = LogManager.Configuration;
            target = new LogServiceTarget(m.Messages);
            target.Layout =
                "${level}=> ${message} ${exception:format=Message} ${exception:format=Type} ${exception:format=StackTrace}";
            target.Name = "LogService";
            var wrapper = new AsyncTargetWrapper(target);

            config.AddTarget("LogService", wrapper);

            rule = new LoggingRule("*", LogLevel.Trace, target);
            config.LoggingRules.Add(rule);

            LogManager.Configuration = config;
            if (Debugger.IsAttached)
                target.Filter = LogLevel.Debug;
        }

        public LogLevel Filter
        {
            set { target.Filter = value; }
            get { return target.Filter; }
        }
    }

    [Target("LogService")]
    public class LogServiceTarget : TargetWithLayout
    {
        private readonly SafeObservedCollection<string> messages;

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryTarget" /> class.
        /// </summary>
        /// <remarks>
        /// The default value of the layout is: <code>${longdate}|${level:uppercase=true}|${logger}|${message}</code>
        /// </remarks>
        public LogServiceTarget(SafeObservedCollection<string> m)
        {
            messages = m;
            Filter = LogLevel.Info;
        }

        /// <summary>
        /// Gets the list of logs gathered in the <see cref="MemoryTarget"/>.
        /// </summary>
        public SafeObservedCollection<string> Logs
        {
            get { return messages; }
        }

        /// <summary>
        /// Hard filter level, changing nlog filters at runtime doesnt seem to work??
        /// </summary>
        public LogLevel Filter { set; get; }

        /// <summary>
        /// Renders the logging event message and adds it to the internal ArrayList of log messages.
        /// </summary>
        /// <param name="logEvent">The logging event.</param>
        protected override void Write(LogEventInfo logEvent)
        {
            if (null != logEvent)
            {
                if (logEvent.Level >= Filter)
                    messages.AddRotate(Layout.Render(logEvent), 50);
            }
        }
    }
}