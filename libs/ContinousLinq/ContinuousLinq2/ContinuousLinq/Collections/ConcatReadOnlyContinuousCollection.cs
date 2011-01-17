using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Collections;

namespace ContinuousLinq.Collections
{
    public class ConcatReadOnlyContinuousCollection<TSource> : ReadOnlyTwoCollectionOperationContinuousCollection<TSource>
    {
        public ConcatReadOnlyContinuousCollection(IList<TSource> first, IList<TSource> second)
            : base(first, second)
        {
            this.NotifyCollectionChangedMonitorForFirst.Add += OnAddToFirst;
            this.NotifyCollectionChangedMonitorForFirst.Remove += OnRemoveFromFirst;
            this.NotifyCollectionChangedMonitorForFirst.Reset += OnResetFirst;
            this.NotifyCollectionChangedMonitorForFirst.Replace += OnReplaceOnFirst;

            this.NotifyCollectionChangedMonitorForSecond.Add += OnAddToSecond;
            this.NotifyCollectionChangedMonitorForSecond.Remove += OnRemoveFromSecond;
            this.NotifyCollectionChangedMonitorForSecond.Reset += OnResetSecond;
            this.NotifyCollectionChangedMonitorForSecond.Replace += OnReplaceOnSecond;
        }

        public override TSource this[int index]
        {
            get { return index < this.First.Count ? this.First[index] : this.Second[index - this.First.Count]; }
            set { throw new AccessViolationException(); }
        }

        public override int Count
        {
            get { return this.First.Count + this.Second.Count; }
        }

        #region First Event Handlers

        void OnAddToFirst(object sender, int index, IEnumerable<TSource> newItems)
        {
            FireAdd(newItems, index);
        }

        void OnRemoveFromFirst(object sender, int index, IEnumerable<TSource> oldItems)
        {
            FireRemove(oldItems, index);
        }

        void OnResetFirst(object sender)
        {
            FireReset();
        }

        void OnReplaceOnFirst(object sender, IEnumerable<TSource> oldItems, int newStartingIndex, IEnumerable<TSource> newItems)
        {
            FireReplace(newItems, oldItems, newStartingIndex);
        }

        #endregion

        #region Second Event Handlers

        void OnAddToSecond(object sender, int index, IEnumerable<TSource> newItems)
        {
            int adjustedIndex = this.First.Count + index;

            FireAdd(newItems, adjustedIndex);
        }

        void OnRemoveFromSecond(object sender, int index, IEnumerable<TSource> oldItems)
        {
            int adjustedIndex = this.First.Count + index;

            FireRemove(oldItems, adjustedIndex);
        }

        void OnResetSecond(object sender)
        {
            FireReset();
        }

        void OnReplaceOnSecond(object sender, IEnumerable<TSource> oldItems, int newStartingIndex, IEnumerable<TSource> newItems)
        {
            int adjustedIndex = this.First.Count + newStartingIndex;

            FireReplace(newItems, oldItems, adjustedIndex);
        }

        #endregion
    }
}