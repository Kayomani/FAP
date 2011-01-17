using System.Collections.Generic;

namespace ContinuousLinq
{
    public abstract class ReadOnlyTwoCollectionOperationContinuousCollection<TSource> : ReadOnlyContinuousCollection<TSource>
    {
        protected IList<TSource> First { get; set; }
        protected IList<TSource> Second { get; set; }

        internal NotifyCollectionChangedMonitor<TSource> NotifyCollectionChangedMonitorForFirst { get; set; }
        internal NotifyCollectionChangedMonitor<TSource> NotifyCollectionChangedMonitorForSecond { get; set; }

        internal ReadOnlyTwoCollectionOperationContinuousCollection(IList<TSource> first, IList<TSource> second)
        {
            this.First = first;
            this.NotifyCollectionChangedMonitorForFirst = new NotifyCollectionChangedMonitor<TSource>(null, first);

            this.Second = second;
            this.NotifyCollectionChangedMonitorForSecond = new NotifyCollectionChangedMonitor<TSource>(null, second);
        }
    }
}