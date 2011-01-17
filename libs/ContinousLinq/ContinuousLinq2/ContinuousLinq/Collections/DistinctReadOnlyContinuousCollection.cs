using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;

namespace ContinuousLinq.Collections
{
    internal class DistinctReadOnlyContinuousCollection<TSource> : ReadOnlyAdapterContinuousCollection<TSource, TSource>
    {
        public ReferenceCountTracker<TSource> ReferenceCountTracker { get; set; }

        public List<TSource> Output { get; set; }

        public DistinctReadOnlyContinuousCollection(IList<TSource> list)
            : base(list)
        {
            this.Output = new List<TSource>();
            this.ReferenceCountTracker = new ReferenceCountTracker<TSource>();

            this.NotifyCollectionChangedMonitor.Add += OnAdd;
            this.NotifyCollectionChangedMonitor.Remove += OnRemove;
            this.NotifyCollectionChangedMonitor.Reset += OnReset;
            this.NotifyCollectionChangedMonitor.Replace += OnReplace;

            int index = 0;
            AddValues(this.Source, out index);
        }

        public override int Count
        {
            get { return this.Output.Count; }
        }

        public override TSource this[int index]
        {
            get { return this.Output[index]; }
            set { throw new AccessViolationException(); }
        }

        private List<TSource> AddValues(IEnumerable<TSource> newItems, out int index)
        {
            index = -1;
            List<TSource> newlyAddedItems = null;
            foreach (TSource item in newItems)
            {
                if (this.ReferenceCountTracker.Add(item))
                {
                    if (newlyAddedItems == null)
                    {
                        newlyAddedItems = new List<TSource>();
                        index = this.Output.Count;
                    }

                    this.Output.Add(item);
                    newlyAddedItems.Add(item);
                }
            }
            return newlyAddedItems;
        }

        private List<TSource> RemoveValues(IEnumerable<TSource> oldItems, out int index)
        {
            index = -1;

            List<TSource> removedItems = null; 
            foreach (TSource item in oldItems)
            {
                if (this.ReferenceCountTracker.Remove(item))
                {
                    if (removedItems == null)
                    {
                        removedItems = new List<TSource>();
                        index = this.Output.IndexOf(item);
                    }

                    this.Output.Remove(item);
                    removedItems.Add(item);
                }
            }
            return removedItems;
        }


        void OnAdd(object sender, int index, IEnumerable<TSource> newItems)
        {
            AddItemsAndNotifyCollectionChanged(newItems);
        }

        private void AddItemsAndNotifyCollectionChanged(IEnumerable<TSource> newItems)
        {
            int indexInOutput;
            List<TSource> newlyAddedItems = AddValues(newItems, out indexInOutput);
            if (newlyAddedItems != null)
            {
                FireAdd(newlyAddedItems, indexInOutput);
            }
        }

        void OnRemove(object sender, int index, IEnumerable<TSource> oldItems)
        {
            RemoveItemsAndNotifyCollectionChanged(oldItems);
        }

        private void RemoveItemsAndNotifyCollectionChanged(IEnumerable<TSource> oldItems)
        {
            int indexInOutput;
            List<TSource> removedItems = RemoveValues(oldItems, out indexInOutput);
            if (removedItems != null)
            {
                FireRemove(removedItems, indexInOutput);
            }
        }

        void OnReset(object sender)
        {
            this.Output.Clear();
            this.ReferenceCountTracker.Clear();
            int indexInOutput;
            AddValues(this.Source, out indexInOutput);
            FireReset();
        }

        void OnReplace(object sender, IEnumerable<TSource> oldItems, int newStartingIndex, IEnumerable<TSource> newItems)
        {
            AddItemsAndNotifyCollectionChanged(newItems);
            RemoveItemsAndNotifyCollectionChanged(oldItems);
        }
    }
}
