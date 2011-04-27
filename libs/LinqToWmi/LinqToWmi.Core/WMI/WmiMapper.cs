using System;
using System.Collections.Generic;
using System.Management;
using System.Reflection;

namespace LinqToWmi.Core.WMI
{
    /// <summary>
    /// Responsible for mapping the management oject to our user type
    /// </summary>
    internal class WmiMapper
    {
        /// <summary>
        /// Responsible for mapping the managementobjects to our entities.
        /// Should use a cached metamap for getting the properties, and an typefactory for deferred instantion of our types
        /// instead of the lazy new T()
        /// </summary>
        public static T MapToEntity<T>(ManagementObject wmiObject) where T : new()
        {
            T instance = new T();

            foreach (PropertyInfo info in typeof(T).GetProperties())
            {
                info.SetValue(instance, MapType(wmiObject.Properties[ConvertMemberName(info)]), null);
            }

            return instance;
        }

        /// <summary>
        /// Maps Management type to windows type 
        /// </summary>
        public static object MapType(PropertyData data)
        {
            if (data.Type == CimType.DateTime)
            {
                return data.Value != null ? ManagementDateTimeConverter.ToDateTime((string)data.Value)
                    : data.Value;
            }
            return data.Value;
        }

        /// <summary>
        /// Extracts the table name 
        /// </summary>
        public static string ConvertTableName(Type type)
        {
            WmiMapAttribute memberAttribute = WmiMapAttribute.Get(type);

            return memberAttribute != null ? memberAttribute.Name : type.Name;
        }

        /// <summary>
        /// Extracts the membername
        /// </summary>
        public static string ConvertMemberName(MemberInfo info)
        {
            WmiMapAttribute memberAttribute = WmiMapAttribute.Get(info);
            return (memberAttribute != null ? memberAttribute.Name : info.Name);
        }
    }
}