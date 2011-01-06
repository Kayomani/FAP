using System;
using System.Collections.Generic;
using System.Text;
using System.Management;

namespace LinqToWmi.ProtoGenerator
{
    /// <summary>
    /// Converter for converting WMI management properties to .NET equivalent
    /// </summary>
    internal class CimTypeConverter
    {
        public static Dictionary<CimType, Type> typemap = new Dictionary<CimType, Type>();

        static CimTypeConverter()
        {
            //Changed: Improved type mapping functionality by ensuring proper mappings
            typemap.Add(CimType.Boolean, typeof(Boolean));
            typemap.Add(CimType.Char16, typeof(String));
            typemap.Add(CimType.DateTime, typeof(DateTime));
            typemap.Add(CimType.None, typeof(Object));
            typemap.Add(CimType.Object, typeof(Object));
            typemap.Add(CimType.SInt8, typeof(Int16));
            typemap.Add(CimType.SInt16, typeof(Int16));
            typemap.Add(CimType.SInt32, typeof(Int32));
            typemap.Add(CimType.SInt64, typeof(Int64));
            typemap.Add(CimType.Reference, typeof(Object));
            typemap.Add(CimType.String, typeof(String));
            typemap.Add(CimType.UInt8, typeof(Byte));
            typemap.Add(CimType.UInt16, typeof(UInt16));
            typemap.Add(CimType.UInt32, typeof(UInt32));
            typemap.Add(CimType.UInt64, typeof(UInt64));
            typemap.Add(CimType.Real32, typeof(Single));
            typemap.Add(CimType.Real64, typeof(Double));
        }

        /// <summary>
        /// Gets the .Net type equivalent for the WMI (CIM) type.
        /// </summary>
        public static Type Convert(PropertyData property) 
        {
            //Added: Support for arrays
            if (!property.IsArray)
            {
                return typemap[property.Type];
            }
            else
            {
                return Type.GetType(String.Format("{0}[]", typemap[property.Type]));
            }
        }
    }
}
