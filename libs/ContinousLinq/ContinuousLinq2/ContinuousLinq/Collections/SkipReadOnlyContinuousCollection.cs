using System;
using System.Collections.Generic;
using System.Linq;

namespace ContinuousLinq.Collections
{
    public class SkipReadOnlyContinuousCollection<TSource> : ReadOnlyAdapterContinuousCollection<TSource, TSource>
    {
        public SkipReadOnlyContinuousCollection(IList<TSource> list, int count)
            : base(list)
        {
            NotifyCollectionChangedMonitor.Add += OnAdd;
            NotifyCollectionChangedMonitor.Remove += OnRemove;
            NotifyCollectionChangedMonitor.Replace += OnReplace;
            NotifyCollectionChangedMonitor.Reset += OnReset;
            NotifyCollectionChangedMonitor.Move += OnMove;
            SkipCount = count;
        }

        internal int SkipCount { get; set; }

        public override TSource this[int index]
        {
            get { return Source[index + SkipCount]; }
            set { throw new AccessViolationException(); }
        }

        public override int Count
        {
            get { return SkipCount < Source.Count ? Source.Count - SkipCount : 0; }
        }

        private IEnumerable<TSource> FirstItemInSkipRange
        {
            get { return Source.Skip(SkipCount).Take(1); }
        }

        private IEnumerable<TSource> FirstItemBeforeSkipRange
        {
            get { return Source.Skip(SkipCount - 1).Take(1); }
        }

        private void OnMove(object sender, int oldStartingIndex, IEnumerable<TSource> oldItems, int newStartingIndex,
                            IEnumerable<TSource> newItems)
        {
            if (ItemWithinSkipRange(oldStartingIndex))
            {
                int skipIndexOfOldIndex = DistanceFromSkipBoundary(oldStartingIndex);
                if (ItemWithinSkipRange(newStartingIndex))
                {
                    FireMove(oldItems.ToList(), DistanceFromSkipBoundary(newStartingIndex), skipIndexOfOldIndex);
                }
                else
                {
                    FireRemove(oldItems, skipIndexOfOldIndex);
                    FireAdd(FirstItemInSkipRange, 0);
                }
            }
            else if (ItemWithinSkipRange(newStartingIndex))
            {
                if (newStartingIndex == SkipCount)
                {
                    FireReplace(oldItems, FirstItemBeforeSkipRange, 0);
                }
                else
                {
                    FireRemove(FirstItemBeforeSkipRange, 0);
                    FireAdd(oldItems, DistanceFromSkipBoundary(newStartingIndex));
                }
            }
        }

        private void OnAdd(object sender, int index, IEnumerable<TSource> newItems)
        {
            bool itemInsertedBeforeSkipRange = ItemIsBeforeSkipRange(index);
            IEnumerable<TSource> addedItems = GetAddedItems(index, newItems);
            if (addedItems != null)
            {
                FireAdd(addedItems, itemInsertedBeforeSkipRange ? 0 : DistanceFromSkipBoundary(index));
            }
        }

        private void OnReplace(object sender, IEnumerable<TSource> oldItems, int newStartingIndex,
                               IEnumerable<TSource> newItems)
        {
            int endIndex = newItems.Count() + newStartingIndex - 1;
            bool startIndexInSkipRange = ItemWithinSkipRange(newStartingIndex);
            bool endIndexInSkipRange = ItemWithinSkipRange(endIndex);
            if (!startIndexInSkipRange && !endIndexInSkipRange)
            {
                return;
            }
            int skipStartIndex = DistanceFromSkipBoundary(newStartingIndex);
            if (startIndexInSkipRange)
            {
                FireReplace(newItems, oldItems, skipStartIndex);
            }
            else
            {
                FireReplace(newItems.Skip(skipStartIndex), oldItems.Skip(skipStartIndex), 0);
            }
        }

        private void OnRemove(object sender, int index, IEnumerable<TSource> oldItems)
        {
            IEnumerable<TSource> removedItems = GetRemovedItems(index, oldItems);

            if (removedItems != null)
            {
                if (Count == 0)
                {
                    FireReset();
                }
                else
                {
                    int removeIndex = ItemWithinSkipRange(index) ? DistanceFromSkipBoundary(index) : 0;
                    FireRemove(removedItems, removeIndex);
                }
            }
        }

        private void OnReset(object sender)
        {
            FireReset();
        }

        private bool ItemWithinSkipRange(int itemIndex)
        {
            return itemIndex >= SkipCount;
        }

        private IEnumerable<TSource> GetAddedItems(int index, IEnumerable<TSource> items)
        {
            if (index > SkipCount)
                return items;
            if (Source.Count < SkipCount)
                return null;
            int itemAddedCount = items.Count();
            return Source.Skip(SkipCount).Take(itemAddedCount);
        }

        private IEnumerable<TSource> GetRemovedItems(int index, IEnumerable<TSource> oldItems)
        {
            int oldItemsCount = oldItems.Count();
            if(Count+oldItemsCount<=SkipCount)
            {
                return null;
            }
            if (ItemWithinSkipRange(index))
            {
                IEnumerator<TSource> enumerator = oldItems.GetEnumerator();
                var removedItems = new List<TSource>();
                for (int sourceIndex = index; enumerator.MoveNext(); sourceIndex++)
                {
                    if (sourceIndex >= SkipCount)
                        removedItems.Add(enumerator.Current);
                }
                return removedItems;
            }
            int countBetweenIndexAndSkip = DistanceFromSkipBoundary(index);
            int indexOfOldSkipBoundaryItem = DistanceFromSkipBoundary(oldItemsCount);
            return countBetweenIndexAndSkip < oldItemsCount
                       ? oldItems.Skip(countBetweenIndexAndSkip)
                             .Concat(Source.Skip(index).Take(DistanceFromSkipBoundary(oldItemsCount - countBetweenIndexAndSkip)))
                       : Source.Skip(indexOfOldSkipBoundaryItem).Take(oldItemsCount);
        }

        private int DistanceFromSkipBoundary(int index)
        {
            return Math.Abs(SkipCount - index);
        }

        private bool ItemIsBeforeSkipRange(int itemIndex)
        {
            return itemIndex < SkipCount;
        }
    }
}