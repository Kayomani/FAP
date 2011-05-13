using System;

namespace HttpServer.Logging
{
    /// <summary>
    /// Factory is used to create new logs in the system.
    /// </summary>
    public static class LogFactory
    {
        private static ILogFactory _factory = NullLogFactory.Instance;
        private static bool _isAssigned;

        /// <summary>
        /// Assigns log factory being used.
        /// </summary>
        /// <param name="logFactory">The log factory.</param>
        /// <exception cref="InvalidOperationException">A factory have already been assigned.</exception>
        public static void Assign(ILogFactory logFactory)
        {
            if (logFactory == _factory)
                return;
            if (_isAssigned)
                throw new InvalidOperationException("A factory have already been assigned.");
            _isAssigned = true;
            _factory = logFactory;
        }

        /// <summary>
        /// Create a new logger.
        /// </summary>
        /// <param name="type">Type that requested a logger.</param>
        /// <returns>Logger for the specified type;</returns>
        public static ILogger CreateLogger(Type type)
        {
            return _factory.CreateLogger(type);
        }
    }
}