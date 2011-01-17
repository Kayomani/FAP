using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.Generic;

namespace System.Collections.Specialized
{
    public class HybridDictionary : Dictionary<object, object>
    {
        public bool Contains(object key)
        {
            return ContainsKey(key);
        }
    }
}
