using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.ComponentModel;
using System.Collections.Specialized;

namespace ContinuousLinq
{
    public class PassThroughReadOnlyContinuousCollection<TSource> : ReadOnlyAdapterContinuousCollection<TSource, TSource>
    {
        public PassThroughReadOnlyContinuousCollection(IList<TSource> list)
            : base(list, null)
        {
            this.NotifyCollectionChangedMonitor.CollectionChanged += OnSourceCollectionChanged;
        }

        void OnSourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            RefireCollectionChanged(args);
        }
        
        public override int Count
        {
            get { return this.Source.Count; }
        }

        public override TSource this[int index]
        {
            get { return this.Source[index]; }
            set { throw new AccessViolationException(); }
        }
    }
}
