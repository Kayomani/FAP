using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Diagnostics;

namespace ContinuousLinq.Collections
{
    public abstract class OrderedReadOnlyContinuousCollection<TSource> : ReadOnlyAdapterContinuousCollection<TSource, TSource>
        where TSource : INotifyPropertyChanged
    {
        internal IComparer<TSource> KeySorter { get; set; }

        protected internal OrderedReadOnlyContinuousCollection(IList<TSource> list, PropertyAccessTree propertyAccessTree)
            : base(list, propertyAccessTree)
        { }
    }
}
