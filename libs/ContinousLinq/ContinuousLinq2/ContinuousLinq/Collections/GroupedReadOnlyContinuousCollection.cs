using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;

namespace ContinuousLinq.Collections
{
    public class GroupedReadOnlyContinuousCollection<TKey, TSource>
        : ReadOnlyAdapterContinuousCollection<TSource, TSource>, IGrouping<TKey, TSource>
    {
        internal ContinuousCollection<TSource> InternalCollection 
        {
            get { return (ContinuousCollection<TSource>)this.Source; } 
        }

        public GroupedReadOnlyContinuousCollection(TKey key)
            : base(new ContinuousCollection<TSource>(), null)
        {
            this.Key = key;
            this.NotifyCollectionChangedMonitor.CollectionChanged += OnSourceCollectionChanged;
        }

        void OnSourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            RefireCollectionChanged(args);
        }

        #region IGrouping<TKey,TSource> Members

        private TKey _key;
        public TKey Key
        {
            get { return _key; }
            set
            {
                if (EqualityComparer<TKey>.Default.Equals(value, _key))
                    return;

                _key = value;
                OnPropertyChanged("Key");
            }
        }

        #endregion

        public override TSource this[int index]
        {
            get { return this.Source[index]; }
            set { throw new AccessViolationException(); }
        }

        public override int Count
        {
            get { return this.Source.Count; }
        }
    }
}
