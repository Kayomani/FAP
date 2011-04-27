using System;

namespace HttpServer.Logging
{
    /// <summary>
    /// Interface used to write to log files.
    /// </summary>
    /// <remarks>
    /// If you want to use the built in filtering mechanism, create a constructor
    /// which takes one parameter, a <see cref="ILogFilter"/>.
    /// </remarks>
    public interface ILogger
    {
        /// <summary>
        /// Write an entry that helps when debugging code.
        /// </summary>
        /// <param name="message">Log message</param>
        void Debug(string message);

        /// <summary>
        /// Write an entry that helps when debugging code.
        /// </summary>
        /// <param name="message">Log message</param>
        /// <param name="exception">Thrown exception to log.</param>
        void Debug(string message, Exception exception);

        /// <summary>
        /// Something went wrong, but the application do not need to die. The current thread/request
        /// cannot continue as expected.
        /// </summary>
        /// <param name="message">Log message</param>
        void Error(string message);

        /// <summary>
        /// Something went wrong, but the application do not need to die. The current thread/request
        /// cannot continue as expected.
        /// </summary>
        /// <param name="message">Log message</param>
        /// <param name="exception">Thrown exception to log.</param>
        void Error(string message, Exception exception);

        /// <summary>
        /// Something went very wrong, application might not recover.
        /// </summary>
        /// <param name="message">Log message</param>
        void Fatal(string message);

        /// <summary>
        /// Something went very wrong, application might not recover.
        /// </summary>
        /// <param name="message">Log message</param>
        /// <param name="exception">Thrown exception to log.</param>
        void Fatal(string message, Exception exception);

        /// <summary>
        /// Informational message, needed when helping customer to find a problem.
        /// </summary>
        /// <param name="message">Log message</param>
        void Info(string message);

        /// <summary>
        /// Informational message, needed when helping customer to find a problem.
        /// </summary>
        /// <param name="message">Log message</param>
        /// <param name="exception">Thrown exception to log.</param>
        void Info(string message, Exception exception);

        /// <summary>
        /// Write a entry that helps when trying to find hard to find bugs.
        /// </summary>
        /// <param name="message">Log message</param>
        void Trace(string message);

        /// <summary>
        /// Write a entry that helps when trying to find hard to find bugs.
        /// </summary>
        /// <param name="message">Log message</param>
        /// <param name="exception">Thrown exception to log.</param>
        void Trace(string message, Exception exception);

        /// <summary>
        /// Something is not as we expect, but the code can continue to run without any changes.
        /// </summary>
        /// <param name="message">Log message</param>
        void Warning(string message);

        /// <summary>
        /// Something is not as we expect, but the code can continue to run without any changes.
        /// </summary>
        /// <param name="message">Log message</param>
        /// <param name="exception">Thrown exception to log.</param>
        void Warning(string message, Exception exception);
    }
}