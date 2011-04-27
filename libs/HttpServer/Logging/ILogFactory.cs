using System;

namespace HttpServer.Logging
{
    /// <summary>
    /// Factory implementation used to create logs.
    /// </summary>
    public interface ILogFactory
    {
        /// <summary>
        /// Create a new logger.
        /// </summary>
        /// <param name="type">Type that requested a logger.</param>
        /// <returns>Logger for the specified type;</returns>
        /// <remarks>
        /// MUST ALWAYS return a logger. Return <see cref="NullLogWriter"/> if no logging
        /// should be used.
        /// </remarks>
        ILogger CreateLogger(Type type);
    }
}