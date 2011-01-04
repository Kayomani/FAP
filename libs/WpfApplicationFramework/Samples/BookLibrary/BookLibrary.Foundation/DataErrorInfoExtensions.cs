using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace BookLibrary.Foundation
{
    public static class DataErrorInfoExtensions
    {
        public static string Validate(this IDataErrorInfo obj)
        {
            if (obj == null) { throw new ArgumentNullException("obj"); }

            return obj.Error;
        }
        
        public static string Validate(this IDataErrorInfo obj, string memberName)
        {
            if (obj == null) { throw new ArgumentNullException("obj"); }

            return obj[memberName];
        }
    }
}
