using System;
using System.Collections.Generic;
using HttpServer.Messages;
using HttpServer.Messages.Parser;

namespace HttpServer.Logging
{
    /// <summary>
    /// Default log filter implementation.
    /// </summary>
    public class LogFilter : ILogFilter
    {
        private readonly List<NamespaceFilter> _namespaces = new List<NamespaceFilter>();
        private readonly Dictionary<Type, LogLevel> _types = new Dictionary<Type, LogLevel>();

        /// <summary>
        /// Add a name space filter.
        /// </summary>
        /// <param name="ns">Name space to add filter for.</param>
        /// <param name="level">Minimum log level required.</param>
        /// <example>
        /// <code>
        /// // Parsing can only add error and fatal messages
        /// AddNamespace("SipSharp.Messages.Headers.Parsers", LogLevel.Error);
        /// AddType(typeof(SipParser), LogLevel.Error);
        /// 
        /// // Transport layer can only log warnings, errors and fatal messages
        /// AddNamespace("SipSharp.Transports.*", LogLevel.Warning);
        /// </code>
        /// </example>
        public void AddNameSpace(string ns, LogLevel level)
        {
            lock (_namespaces)
                _namespaces.Add(new NamespaceFilter(ns, level));
        }

        /// <summary>
        /// Used to specify standard filter rules
        /// </summary>
        /// <remarks>
        /// Parser can only display errors. Transports only warnings.
        /// </remarks>
        public void AddStandardRules()
        {
            AddNameSpace("HttpServer.Headers.Parsers", LogLevel.Error);
            AddType(typeof (HttpParser), LogLevel.Error);
            AddType(typeof (MessageFactory), LogLevel.Error);

            //bool found = false;
            //foreach (NamespaceFilter ns in _namespaces)
            //{
            //    //if (!ns.NameSpace.StartsWith("SipSharp.Transports")) continue;
            //    found = true;
            //    break;
            //}
            //if (!found)
            //    AddNamespace("SipSharp.Transports.*", LogLevel.Warning);
        }

        /// <summary>
        /// Add filter for a type
        /// </summary>
        /// <param name="type">Type to add filter for.</param>
        /// <param name="level">Minimum log level required.</param>
        /// <example>
        /// <code>
        /// // Parsing can only add error and fatal messages
        /// AddNamespace("SipSharp.Messages.Headers.Parsers", LogLevel.Error);
        /// AddType(typeof(SipParser), LogLevel.Error);
        /// 
        /// // Transport layer can only log warnings, errors and fatal messages
        /// AddNamespace("SipSharp.Transports.*", LogLevel.Warning);
        /// </code>
        /// </example>
        public void AddType(Type type, LogLevel level)
        {
            lock (_types)
                _types.Add(type, level);
        }

        /// <summary>
        /// Add filter for a type
        /// </summary>
        /// <param name="typeStr">Type to add filter for.</param>
        /// <param name="level">Minimum log level required.</param>
        /// <example>
        /// <code>
        /// // Parsing can only add error and fatal messages
        /// AddNamespace("SipSharp.Messages.Headers.Parsers", LogLevel.Error);
        /// AddType("SipSharp.Messages.MessageFactory", LogLevel.Error);
        /// 
        /// // Transport layer can only log warnings, errors and fatal messages
        /// AddNamespace("SipSharp.Transports.*", LogLevel.Warning);
        /// </code>
        /// </example>
        /// <exception cref="ArgumentException">Type could not be identified.</exception>
        public void AddType(string typeStr, LogLevel level)
        {
            Type type = Type.GetType(typeStr);
            if (type == null)
                throw new ArgumentException("Type could not be identified.");

            lock (_types)
                _types.Add(type, level);
        }

        #region ILogFilter Members

        /// <summary>
        /// Checks if the specified type can send
        /// log entries at the specified level.
        /// </summary>
        /// <param name="level">Log level</param>
        /// <param name="type">Type that want to write a log entry.</param>
        /// <returns><c>true</c> if logging is allowed; otherwise <c>false</c>.</returns>
        bool ILogFilter.CanLog(LogLevel level, Type type)
        {
            lock (_types)
            {
                LogLevel allowedLevel;
                if (_types.TryGetValue(type, out allowedLevel))
                    return level >= allowedLevel;
            }

            lock (_namespaces)
            {
                foreach (NamespaceFilter filter in _namespaces)
                {
                    if (filter.IsWildcard)
                        if (type.Namespace.StartsWith(filter.NameSpace))
                            return level >= filter.Level;
                    if (filter.NameSpace.Equals(type.Namespace))
                        return level >= filter.Level;
                }
            }

            return true;
        }

        #endregion

        #region Nested type: NamespaceFilter

        private class NamespaceFilter
        {
            /// <exception cref="ArgumentException">No filters = everything logged. <see cref="NullLogFactory"/> = no logs. Don't use a rule with '*' or '.*'</exception>
            public NamespaceFilter(string ns, LogLevel level)
            {
                if (ns == "*" || ns == ".*")
                    throw new ArgumentException(
                        "No filters = everything logged. NullLogFactory = no logs. Don't use a rule with '*' or '.*'");

                NameSpace = ns;
                int pos = NameSpace.IndexOf('*');
                if (pos > 0)
                {
                    NameSpace = NameSpace[pos - 1] == '.' ? NameSpace.Remove(pos - 1) : NameSpace.Remove(pos);
                    IsWildcard = true;
                }
                Level = level;
            }

            /// <summary>
            /// User have specified a wild card filter.
            /// </summary>
            /// <remarks>
            /// Wild card filters are used to log a name space and
            /// all it's children name spaces.
            /// </remarks>
            public bool IsWildcard { get; private set; }

            public LogLevel Level { get; private set; }
            public string NameSpace { get; private set; }
        }

        #endregion
    }
}