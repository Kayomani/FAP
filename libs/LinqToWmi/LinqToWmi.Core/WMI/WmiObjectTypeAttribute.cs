using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace LinqToWmi.Core.WMI
{
    /// <summary>
    /// Provides type decoration for the WMI object
    /// </summary>
    public class WmiMapAttribute : Attribute
    {

        private string _name;

        public WmiMapAttribute(string name)
        {
            _name = name;
        }

        /// <summary>
        /// Returns the name of the current WMI m
        /// </summary>
        public string Name
        {
            get
            {
                return _name;
            }
        }

        /// <summary>
        /// Retrieves the attribute from an type
        /// </summary>
        public static WmiMapAttribute Get(Type type)
        {
            return (WmiMapAttribute)WmiMapAttribute.GetCustomAttribute(type, typeof(WmiMapAttribute));
        }

        /// <summary>
        /// Extracts the attribute from an MemberInfo
        /// </summary>
        public static WmiMapAttribute Get(MemberInfo info)
        {
            return (WmiMapAttribute)WmiMapAttribute.GetCustomAttribute(info, typeof(WmiMapAttribute));
        }
    }
}
