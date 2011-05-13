using System;
using System.Collections.Generic;
using HttpServer.Tools.Properties;

namespace HttpServer.Helpers
{
    /// <summary>
    /// Assign properties from HTTP parameters.
    /// </summary>
    public class PropertyAssigner
    {
        private static FilterHandler _handler;

        /// <summary>
        /// Used to filter out properties.
        /// </summary>
        /// <param name="handler">Filter handler.</param>
        /// <exception cref="InvalidOperationException">Handler have already been set.</exception>
        public static void SetFilterHandler(FilterHandler handler)
        {
            if (_handler != null)
                throw new InvalidOperationException("Handler have already been set.");

            _handler = handler;
        }

        /// <summary>
        /// Assign properties in the specified object.
        /// </summary>
        /// <param name="instance">Object to fill.</param>
        /// <param name="parameters">Contains all parameters that should be assigned to the properties.</param>
        /// <exception cref="PropertyException">Properties was not found or value could not be converted.</exception>
        /// <exception cref="ArgumentNullException">Any parameter is <c>null</c>.</exception>
        public static void Assign(object instance, IParameterCollection parameters)
        {
            if (instance == null)
                throw new ArgumentNullException("instance");
            if (parameters == null)
                throw new ArgumentNullException("parameters");
            var errors = new Dictionary<string, Exception>();
            ICachedType type = PropertyProvider.Get(instance.GetType());
            foreach (IParameter parameter in parameters)
            {
                try
                {
                    object value = parameter.Value;
                    if (_handler != null && !_handler(instance, parameter.Name, ref value))
                        continue;

                    type.SetConvertedValue(instance, parameter.Name, value);
                }
                catch (Exception err)
                {
                    errors[parameter.Name] = err;
                }
            }

            if (errors.Count != 0)
                throw new PropertyException(errors);
        }
    }

    /// <summary>
    /// Used to be able to filter properties
    /// </summary>
    /// <param name="instance">Model having it's properties assigned</param>
    /// <param name="propertyName">Property about to be assigned</param>
    /// <param name="propertyValue">Value to assign</param>
    /// <returns><c>true</c> if value can be set; otherwise <c>false</c>.</returns>
    public delegate bool FilterHandler(object instance, string propertyName, ref object propertyValue);

    /// <summary>
    /// Failed to assign properties.
    /// </summary>
    public class PropertyException : Exception
    {
        private readonly Dictionary<string, Exception> _propertyErrors;

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyException"/> class.
        /// </summary>
        /// <param name="propertyErrors">The property errors.</param>
        public PropertyException(Dictionary<string, Exception> propertyErrors)
        {
            _propertyErrors = propertyErrors;
        }

        /// <summary>
        /// Gets all errors during assignment.
        /// </summary>
        /// <remarks>
        /// Dictionary key is property name.
        /// </remarks>
        public Dictionary<string, Exception> PropertyErrors
        {
            get { return _propertyErrors; }
        }
    }
}