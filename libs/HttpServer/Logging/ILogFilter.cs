using System;

namespace HttpServer.Logging
{
    /// <summary>
    /// Determines which classes can log
    /// </summary>
    public interface ILogFilter
    {
        /// <summary>
        /// Checks if the specified type can send
        /// log entries at the specified level.
        /// </summary>
        /// <param name="level">Log level</param>
        /// <param name="type">Type that want to write a log entry.</param>
        /// <returns><c>true</c> if logging is allowed; otherwise <c>false</c>.</returns>
        bool CanLog(LogLevel level, Type type);
    }
}