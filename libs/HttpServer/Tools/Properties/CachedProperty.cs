using System;
using System.Collections.Generic;
using System.Reflection;
using HttpServer.Helpers;

namespace HttpServer.Tools.Properties
{
    /// <summary>
    /// Type cached for fast property value modifications.
    /// </summary>
    public interface ICachedType
    {
        /// <summary>
        /// Get a property value.
        /// </summary>
        /// <param name="instance">Instance to get value from.</param>
        /// <param name="propertyName">Name of property.</param>
        /// <returns>Property value.</returns>
        object GetValue(object instance, string propertyName);

        /// <summary>
        /// Assign a value, try to convert it if it's not the same type as the property type.
        /// </summary>
        /// <param name="instance">Object containing the property</param>
        /// <param name="propertyName">Name of property</param>
        /// <param name="value">Value to convert and assign</param>
        /// <exception cref="InvalidOperationException">Failed to find property.</exception>
        /// <exception cref="InvalidCastException">Could not convert value type to property type.</exception>
        void SetConvertedValue(object instance, string propertyName, object value);

        /// <summary>
        /// Assign value to a property
        /// </summary>
        /// <param name="instance">Object containing the property</param>
        /// <param name="propertyName">Name of property</param>
        /// <param name="value">Value to assign, must be of the same type as the property.</param>
        /// <exception cref="InvalidOperationException">Failed to find property.</exception>
        void SetValue(object instance, string propertyName, object value);
    }

    /// <summary>
    /// Used to cache property information
    /// </summary>
    internal class CachedType : ICachedType
    {
        private readonly Dictionary<string, CachedProperty> _properties =
            new Dictionary<string, CachedProperty>(StringComparer.OrdinalIgnoreCase);

        private readonly EmitReflector _reflectorProvider;
        private readonly Type _type;
        static private Dictionary<Type, object> _valueTypeDefaults = new Dictionary<Type, object>();

        public CachedType(Type type, EmitReflector reflectorProvider)
        {
            foreach (PropertyInfo property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (!property.CanRead || !property.CanWrite)
                    continue;

                ConvertTypeHandler handler;
                if (property.PropertyType == typeof(string))
                    handler = ToString;
                else if (property.PropertyType == typeof(DateTime))
                    handler = ToDate;
                else if (property.PropertyType.IsEnum)
                    handler = ToEnum;
                else if (property.PropertyType == typeof(bool))
                    handler = ToBoolean;
                else
                    handler = ConvertValue;

                _properties.Add(property.Name, new CachedProperty
                                                   {
                                                       Convert = handler,
                                                       MemberInfo = property,
                                                       MemberType = property.PropertyType
                                                   });
            }
            _type = type;
            _reflectorProvider = reflectorProvider;
        }

        private object ToBoolean(Type totype, object value)
        {
            if (value is bool)
                return value;
            if (value is string)
            {
                string temp = ((string) value).ToLower();
                return temp == "1" || temp == "on" || temp == "true";
            }

            throw new InvalidCastException("Cannot convert to boolean from type '" + value.GetType() + "'.");
        }

        private object ConvertValue(Type toType, object value)
        {
			if (value is string && string.IsNullOrEmpty((string)value))
				return null;

            return Convert.ChangeType(value, toType);
        }

        /// <summary>
        /// Gets the property.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">Failed to find property.</exception>
        private CachedProperty GetProperty(string name)
        {
            CachedProperty cachedProperty;
            if (!_properties.TryGetValue(name, out cachedProperty))
                return null;
            return cachedProperty;
        }

        /// <exception cref="InvalidCastException"><c>InvalidCastException</c>.</exception>
        private object ToDate(Type toType, object value)
        {
            if (value is string && (string.IsNullOrEmpty((string)value)))
                return DateTime.MinValue;
            if (value is string)
                return DateTime.Parse((string) value);
            if (value is DateTime)
                return value;

            throw new InvalidCastException("Cannot cast " + value.GetType() + " to DateTime");
        }

        private object ToEnum(Type toType, object value)
        {
            int numericValue;

            return int.TryParse(value.ToString(), out numericValue)
                       ? Enum.ToObject(toType, numericValue)
                       : Enum.Parse(toType, value.ToString());
        }

        private object ToString(Type toType, object value)
        {
            return value.ToString();
        }

        #region ICachedType Members

        /// <summary>
        /// Get a property value.
        /// </summary>
        /// <param name="instance">Instance to get value from.</param>
        /// <param name="propertyName">Name of property.</param>
        /// <returns>Property value.</returns>
        public object GetValue(object instance, string propertyName)
        {
            CachedProperty property = GetProperty(propertyName);
            if (property == null)
                return null;

            return _reflectorProvider.GetValue(property.MemberInfo, instance);
        }

        /// <summary>
        /// Assign a value, try to convert it if it's not the same type as the property type.
        /// </summary>
        /// <param name="instance">Object containing the property</param>
        /// <param name="propertyName">Name of property</param>
        /// <param name="value">Value to convert and assign</param>
        /// <exception cref="InvalidOperationException">Failed to find property.</exception>
        /// <exception cref="InvalidCastException">Could not convert value type to property type.</exception>
        public void SetConvertedValue(object instance, string propertyName, object value)
        {
            CachedProperty property = GetProperty(propertyName);
            if (property == null)
                return;

            Type targetType = property.MemberType;
            if (!targetType.IsAssignableFrom(value.GetType()))
                value = property.Convert(targetType, value);

            // Value types cannot be null, let's get the default value for the value type.
            // Not the nicest solution, feel free to improve it.
            if (value == null && targetType.IsValueType)
            {
                object defaultValue;
                lock (_valueTypeDefaults)
                {
                    if (!_valueTypeDefaults.TryGetValue(targetType, out defaultValue))
                    {
                        defaultValue = Activator.CreateInstance(targetType);
                        _valueTypeDefaults[targetType] = defaultValue;
                    }
                }

                value = defaultValue;
            }

			_reflectorProvider.SetValue(property.MemberInfo, instance, value);
        }

        /// <summary>
        /// Assign value to a property
        /// </summary>
        /// <param name="instance">Object containing the property</param>
        /// <param name="propertyName">Name of property</param>
        /// <param name="value">Value to assign, must be of the same type as the property.</param>
        /// <exception cref="InvalidOperationException">Failed to find property.</exception>
        public void SetValue(object instance, string propertyName, object value)
        {
            CachedProperty property = GetProperty(propertyName);
            if (property == null)
                return;

            _reflectorProvider.SetValue(property.MemberInfo, instance, value);
        }

        #endregion

        #region Nested type: CachedProperty

        private class CachedProperty
        {
            public ConvertTypeHandler Convert;

            /// <summary>
            /// Gets or sets member info
            /// </summary>
            public MemberInfo MemberInfo { get; set; }

            /// <summary>
            /// Gets or sets member type
            /// </summary>
            public Type MemberType { get; set; }
        }

        #endregion

        #region Nested type: ConvertTypeHandler

        private delegate object ConvertTypeHandler(Type toType, object value);

        #endregion
    }
}