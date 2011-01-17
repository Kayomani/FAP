using System;
using System.Collections.Generic;
using System.Linq;

namespace ContinuousLinq.Collections
{
    internal struct ItemsIndex<TSource>
    {
        public int Index;
        public IEnumerable<TSource> Items;
    }

    public class TakeReadOnlyContinuousCollection<TSource> : ReadOnlyAdapterContinuousCollection<TSource, TSource>
    {
        public TakeReadOnlyContinuousCollection(IList<TSource> list, int count)
            : base(list)
        {
            NotifyCollectionChangedMonitor.Add += OnAdd;
            NotifyCollectionChangedMonitor.Remove += OnRemove;
            NotifyCollectionChangedMonitor.Replace += OnReplace;
            NotifyCollectionChangedMonitor.Reset += OnReset;
            NotifyCollectionChangedMonitor.Move += OnMove;
            TakeCount = count;
        }

        internal int TakeCount { get; set; }

        public override TSource this[int index]
        {
            get { return Source[index]; }
            set { throw new AccessViolationException(); }
        }

        public override int Count
        {
            get { return TakeCount > Source.Count ? Source.Count : TakeCount; }
        }

        private IEnumerable<TSource> LastItemInTakeRange
        {
            get { return Source.Skip(TakeCount - 1).Take(1); }
        }

        private IEnumerable<TSource> FirstItemOutsideOfTakeRange
        {
            get { return Source.Skip(TakeCount).Take(1); }
        }

        private int LastItemIndexInTakeRange
        {
            get { return TakeCount - 1; }
        }

        private void OnMove(object sender, int oldStartingIndex, IEnumerable<TSource> oldItems, int newStartingIndex,
                            IEnumerable<TSource> newItems)
        {
            bool itemMovedFromTake = IndexWithinTakeRange(oldStartingIndex);
            bool itemMovedToTake = IndexWithinTakeRange(newStartingIndex);
            if (!itemMovedFromTake && !itemMovedToTake)
                return;

            if (itemMovedFromTake && itemMovedToTake)
            {
                FireMove(newItems.ToList(), newStartingIndex, oldStartingIndex);
            }
            else if (itemMovedToTake)
            {
                FireRemove(FirstItemOutsideOfTakeRange, LastItemIndexInTakeRange);
                FireAdd(newItems, newStartingIndex);
            }
            else
            {
                FireRemove(oldItems, oldStartingIndex);
                FireAdd(LastItemInTakeRange, LastItemIndexInTakeRange);
            }
        }

        private void OnAdd(object sender, int index, IEnumerable<TSource> newItems)
        {
            IEnumerable<TSource> addedItems = GetAddedItems(index, newItems);
            ItemsIndex<TSource>? removedItems = GetRemovedItemsFromAdd(index, newItems.Count());
            if (addedItems != null)
            {
                if (removedItems != null)
                    FireRemove(removedItems.Value.Items, removedItems.Value.Index);
                FireAdd(addedItems, index);
            }
        }

        private void OnReplace(object sender, IEnumerable<TSource> oldItems, int newStartingIndex,
                               IEnumerable<TSource> newItems)
        {
            IEnumerable<TSource> addedItems = GetAddedItems(newStartingIndex, newItems);
            if (addedItems != null)
                FireReplace(addedItems, oldItems, newStartingIndex);
        }

        private void OnRemove(object sender, int index, IEnumerable<TSource> oldItems)
        {
            IEnumerable<TSource> removed = GetRemovedItems(index, oldItems);
            if (removed != null)
            {
                FireRemove(removed, index);
                ItemsIndex<TSource>? addedItems = GetAddedItemsFromRemove(index, oldItems.Count());
                if (addedItems != null)
                    FireAdd(addedItems.Value.Items, addedItems.Value.Index);
            }
        }

        private void OnReset(object sender)
        {
            FireReset();
        }

        private bool IndexWithinTakeRange(int itemIndex)
        {
            return itemIndex < TakeCount;
        }


        private IEnumerable<TSource> GetAddedItems(int index, IEnumerable<TSource> items)
        {
            if (!IndexWithinTakeRange(index))
                return null;
            return items.Take(TakeCount - index);
        }

        private IEnumerable<TSource> GetRemovedItems(int index, IEnumerable<TSource> items)
        {
            if (!IndexWithinTakeRange(index))
                return null;
            IEnumerator<TSource> enumerator = items.GetEnumerator();
            var removedItems = new List<TSource>();
            for (int sourceIndex = index; enumerator.MoveNext() && sourceIndex < TakeCount; sourceIndex++)
            {
                removedItems.Add(enumerator.Current);
            }
            return removedItems;
        }

        private ItemsIndex<TSource>? GetRemovedItemsFromAdd(int addedItemsIndex, int addedItemsCount)
        {
            if (InsertedItemInSourceAndNoItemsPushedOutOfTake(addedItemsIndex, addedItemsCount))
            {
                return null;
            }
            int take, skip, index;
            if (InsertedItemsInSourceThatExtendBeyondTakeRange(addedItemsIndex, addedItemsCount))
            {
                skip = addedItemsIndex + addedItemsCount;
                take = TakeCount - addedItemsIndex;
                index = addedItemsIndex;
            }
            else
            {
                skip = TakeCount;
                take = addedItemsCount;
                index = TakeCount - addedItemsCount;
            }

            return new ItemsIndex<TSource> {Items = Source.Skip(skip).Take(take), Index = index};
        }

        private ItemsIndex<TSource>? GetAddedItemsFromRemove(int removedItemsIndex, int removedItemsCount)
        {
            if (LessThanTakeCountItemsBeforeRemove(removedItemsCount))
            {
                return null;
            }
            int addedIndex;
            int addedItemsCount;
            if (SomeItemsRemovedOutsideTakeRange(removedItemsIndex, removedItemsCount))
            {
                addedIndex = removedItemsIndex;
                addedItemsCount = Count - removedItemsIndex;
            }
            else
            {
                addedIndex = TakeCount - removedItemsCount;
                addedItemsCount = removedItemsCount;
            }
            return new ItemsIndex<TSource> {Items = Source.Skip(addedIndex).Take(addedItemsCount), Index = addedIndex};
        }

        private bool LessThanTakeCountItemsBeforeRemove(int removedItemsCount)
        {
            return IndexWithinTakeRange(Count + removedItemsCount - 1);
        }

        private bool SomeItemsRemovedOutsideTakeRange(int removedItemsIndex, int removedItemsCount)
        {
            return !IndexWithinTakeRange(removedItemsIndex + removedItemsCount - 1);
        }

        private bool InsertedItemInSourceAndNoItemsPushedOutOfTake(int addedItemsIndex, int addedItemsCount)
        {
            return !IndexWithinTakeRange(addedItemsIndex) ||
                   IndexWithinTakeRange(Source.Count - addedItemsCount);
        }

        private bool InsertedItemsInSourceThatExtendBeyondTakeRange(int addedItemsIndex, int addedItemsCount)
        {
            return !IndexWithinTakeRange(addedItemsCount + addedItemsIndex);
        }
    }
}