using System.Collections.Generic;

namespace ContinuousLinq
{
    public abstract class ReadOnlyAdapterContinuousCollection<TSource, TResult> : ReadOnlyContinuousCollection<TResult>
    {
        protected IList<TSource> Source { get; set; }

        internal NotifyCollectionChangedMonitor<TSource> NotifyCollectionChangedMonitor { get; set; }

        internal ReadOnlyAdapterContinuousCollection(IList<TSource> list, PropertyAccessTree propertyAccessTree)
        {
            this.Source = list;
            this.NotifyCollectionChangedMonitor = new NotifyCollectionChangedMonitor<TSource>(propertyAccessTree, list);
        }

        internal ReadOnlyAdapterContinuousCollection(IList<TSource> list)
            :this(list, null)
        {
        }
    }
}
