using System;

namespace HttpServer.Logging
{
    /// <summary>
    /// Creates a console logger.
    /// </summary>
    public class ConsoleLogFactory : ILogFactory
    {
        private readonly ILogFilter _filter;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConsoleLogFactory"/> class.
        /// </summary>
        /// <param name="filter">The filter.</param>
        public ConsoleLogFactory(ILogFilter filter)
        {
            _filter = filter;
        }

        #region ILogFactory Members

        /// <summary>
        /// Create a new logger.
        /// </summary>
        /// <param name="type">Type that requested a logger.</param>
        /// <returns>Logger for the specified type;</returns>
        /// <remarks>
        /// MUST ALWAYS return a logger. Return <see cref="NullLogWriter"/> if no logging
        /// should be used.
        /// </remarks>
        public ILogger CreateLogger(Type type)
        {
            return new ConsoleLogger(type, _filter);
        }

        #endregion
    }
}