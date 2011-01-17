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
    public class HashSet<T> : Dictionary<T, object>
    {
        public bool Contains(T item)
        {
            return this.ContainsKey(item);
        }

        public void Add(T item)
        {
            this[item] = null;
        }
    }
}