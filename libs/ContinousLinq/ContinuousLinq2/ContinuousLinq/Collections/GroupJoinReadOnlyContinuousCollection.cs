using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.ComponentModel;
using System.Collections.Specialized;
using ContinuousLinq.Expressions;
using ContinuousLinq;

namespace ContinuousLinq.Collections
{
    public class GroupJoinReadOnlyContinuousCollection<TOuter, TInner, TKey, TResult> : ReadOnlyContinuousCollection<TResult>
    {
        private class OuterEntry
        {
            public TOuter Outer { get; set; }
            public TKey Key { get; set; }
            public TResult Result { get; set; }
            public ContinuousCollection<TInner> MatchingInners { get; set; }
            public ReadOnlyContinuousCollection<TInner> MatchingInnersReadOnly { get; set; }

            public OuterEntry(TOuter outer, TKey key)
            {
                this.Key = key;
                this.MatchingInners = new ContinuousCollection<TInner>();
                this.MatchingInnersReadOnly = this.MatchingInners.AsReadOnly();
            }
        }

        public class InnerEntry
        {
            public TKey Key { get; set; }
            public int ReferenceCount { get; set; }

            public InnerEntry(TKey key)
            {
                Key = key;
            }
        }

        private IList<TInner> _inner;
        private IList<TOuter> _outer;

        private Func<TInner, TKey> _innerKeySelector;
        private Func<TOuter, TKey> _outerKeySelector;

        private NotifyCollectionChangedMonitor<TOuter> _outerNotifyCollectionChangedMonitor;
        private NotifyCollectionChangedMonitor<TInner> _innerNotifyCollectionChangedMonitor;

        private ReferenceCountTracker<TOuter> _outerReferenceTracker;

        private Dictionary<TKey, List<OuterEntry>> _keyToOuterLookup;
        private Dictionary<TOuter, OuterEntry> _outerEntries;

        private Dictionary<TKey, List<TInner>> _keyToInnerLookup;
        private Dictionary<TInner, InnerEntry> _innerEntries;

        private Func<TOuter, ReadOnlyContinuousCollection<TInner>, TResult> _resultSelector;

        public override int Count
        {
            get { return _outer.Count; }
        }

        public override TResult this[int index]
        {
            get { return _outerEntries[_outer[index]].Result; }
            set { throw new InvalidOperationException(); }
        }

        public GroupJoinReadOnlyContinuousCollection(
            IList<TOuter> outer,
            IList<TInner> inner,
            Expression<Func<TOuter, TKey>> outerKeySelector,
            Expression<Func<TInner, TKey>> innerKeySelector,
            Expression<Func<TOuter, ReadOnlyContinuousCollection<TInner>, TResult>> resultSelector)
        {
            _outer = outer;
            _inner = inner;
            
            _outerKeySelector = outerKeySelector.CachedCompile();
            _innerKeySelector = innerKeySelector.CachedCompile();
            _resultSelector = resultSelector.CachedCompile();

            _keyToOuterLookup = new Dictionary<TKey, List<OuterEntry>>(_outer.Count);
            _outerEntries = new Dictionary<TOuter, OuterEntry>(_outer.Count);

            _keyToInnerLookup = new Dictionary<TKey, List<TInner>>(_inner.Count);
            _innerEntries = new Dictionary<TInner, InnerEntry>();

            _outerReferenceTracker = new ReferenceCountTracker<TOuter>(outer);

            RebuildAll();

            _outerNotifyCollectionChangedMonitor = new NotifyCollectionChangedMonitor<TOuter>(ExpressionPropertyAnalyzer.Analyze(outerKeySelector), outer);
            _outerNotifyCollectionChangedMonitor.Add += OnOuterAdd;
            _outerNotifyCollectionChangedMonitor.Remove += OnOuterRemove;
            _outerNotifyCollectionChangedMonitor.Reset += OnOuterReset;
            _outerNotifyCollectionChangedMonitor.Move += OnOuterMove;
            _outerNotifyCollectionChangedMonitor.Replace += OnOuterItemReplace;
            _outerNotifyCollectionChangedMonitor.ItemChanged += OnOuterKeyChanged;

            _innerNotifyCollectionChangedMonitor = new NotifyCollectionChangedMonitor<TInner>(ExpressionPropertyAnalyzer.Analyze(innerKeySelector), inner);
            _innerNotifyCollectionChangedMonitor.Add += OnInnerAdd;
            _innerNotifyCollectionChangedMonitor.Remove += OnInnerRemove;
            _innerNotifyCollectionChangedMonitor.Reset += OnInnerReset;
            _innerNotifyCollectionChangedMonitor.Move += OnInnerMove;
            _innerNotifyCollectionChangedMonitor.Replace += OnInnerItemReplace;
            _innerNotifyCollectionChangedMonitor.ItemChanged += OnInnerItemChanged;
        }

        private void RebuildAll()
        {
            RebuildAllInner();
            RebuildAllOuter();
        }

        private void RebuildAllInner()
        {
            for (int i = 0; i < _inner.Count; i++)
            {
                var innerItem = _inner[i];
                TKey innerItemKey = _innerKeySelector(innerItem);

                var innersMatchingKey = _keyToInnerLookup.GetOrCreate(innerItemKey, () => new List<TInner>(1));
                innersMatchingKey.Add(innerItem);

                AddInnerItemEntry(innerItem, innerItemKey);
            }
        }

        private void AddInnerItemEntry(TInner innerItem, TKey innerItemKey)
        {
            InnerEntry innerEntry;
            if (!_innerEntries.TryGetValue(innerItem, out innerEntry))
            {
                innerEntry = new InnerEntry(innerItemKey);
                _innerEntries[innerItem] = innerEntry;
            }

            innerEntry.ReferenceCount++;
        }

        private void RebuildAllOuter()
        {
            for (int i = 0; i < _outer.Count; i++)
            {
                var outerItem = _outer[i];
                RecordOuter(outerItem);
            }
        }

        private OuterEntry RecordOuter(TOuter outerItem)
        {
            _outerReferenceTracker.Add(outerItem);

            OuterEntry outerEntry;
            if (!_outerEntries.TryGetValue(outerItem, out outerEntry))
            {
                TKey outerKey = _outerKeySelector(outerItem);

                outerEntry = new OuterEntry(outerItem, outerKey);

                outerEntry.Result = _resultSelector(outerItem, outerEntry.MatchingInnersReadOnly);

                _outerEntries[outerItem] = outerEntry;

                var outersMatchingKey = _keyToOuterLookup.GetOrCreate(outerKey, () => new List<OuterEntry>(1));
                outersMatchingKey.Add(outerEntry);

                AddAllInnersMatchingKey(outerEntry);
            }

            return outerEntry;
        }

        private void AddAllInnersMatchingKey(OuterEntry outerEntry)
        {
            List<TInner> innersMatchingKey;
            if (_keyToInnerLookup.TryGetValue(outerEntry.Key, out innersMatchingKey))
            {
                for (int i = 0; i < innersMatchingKey.Count; i++)
                {
                    var innerItem = innersMatchingKey[i];
                    outerEntry.MatchingInners.Add(innerItem);
                }
            }
        }

        private OuterEntry RemoveOuter(TOuter outerItem)
        {
            OuterEntry outerEntry = _outerEntries[outerItem];

            if (_outerReferenceTracker.Remove(outerItem))
            {
                _outerEntries.Remove(outerItem);
                            
                List<OuterEntry> outersMatchingKey = _keyToOuterLookup[outerEntry.Key];
                if (outersMatchingKey.Count == 1)
                {
                    _keyToOuterLookup.Remove(outerEntry.Key);
                }
                else
                {
                    outersMatchingKey.Remove(outerEntry);
                }
            }

            return outerEntry;
        }

        private void OnOuterKeyChanged(object sender, INotifyPropertyChanged itemThatChanged)
        {
            TOuter item = (TOuter)itemThatChanged;

            var entryForItem = _outerEntries[item];
            entryForItem.MatchingInners.Clear();

            entryForItem.Key = _outerKeySelector(item);
            AddAllInnersMatchingKey(entryForItem);
        }

        private void OnOuterAdd(object sender, int index, IEnumerable<TOuter> newItems)
        {
            List<TResult> newResults = new List<TResult>(1);
            foreach (var item in newItems)
            {
                OuterEntry entry = RecordOuter(item);
                newResults.Add(entry.Result);
            }

            FireAdd(newResults, index);
        }

        private void OnOuterRemove(object sender, int index, IEnumerable<TOuter> oldItems)
        {
            List<TResult> oldResults = new List<TResult>(1);
            foreach (var item in oldItems)
            {
                OuterEntry entry = RemoveOuter(item);
                oldResults.Add(entry.Result);
            }

            FireRemove(oldResults, index);
        }

        void OnOuterReset(object sender)
        {
            _outerReferenceTracker.Clear();

            _keyToOuterLookup.Clear();
            _outerEntries.Clear();

            RebuildAll();

            FireReset();
        }

        void OnOuterMove(object sender, int oldStartingIndex, IEnumerable<TOuter> oldItems, int newStartingIndex, IEnumerable<TOuter> newItems)
        {
            List<TResult> movedResults = new List<TResult>(1);
            foreach (var item in newItems)
            {
                OuterEntry entryForItem = _outerEntries[item];
                movedResults.Add(entryForItem.Result);
            }

            FireMove(movedResults, newStartingIndex, oldStartingIndex);
        }

        void OnOuterItemReplace(object sender, IEnumerable<TOuter> oldItems, int newStartingIndex, IEnumerable<TOuter> newItems)
        {
            List<TResult> oldResults = new List<TResult>(1);
            foreach (var item in oldItems)
            {
                OuterEntry entry = RemoveOuter(item);
                oldResults.Add(entry.Result);
            }

            List<TResult> newResults = new List<TResult>(1);
            foreach (var item in newItems)
            {
                OuterEntry entry = RecordOuter(item);
                newResults.Add(entry.Result);
            }

            FireReplace(newResults, oldResults, newStartingIndex);
        }

        void OnInnerItemChanged(object sender, INotifyPropertyChanged itemThatChanged)
        {
            TInner innerItem = (TInner)itemThatChanged;
            var entryForItem = _innerEntries[innerItem];

            TKey newKey = _innerKeySelector(innerItem);
            TKey oldKey = entryForItem.Key;

            for (int i = 0; i < entryForItem.ReferenceCount; i++)
            {
                RemoveInnerItemFromAllOuterEntriesMatchingKey(innerItem, entryForItem.Key);
                RemoveInnerItemFromKeyLookup(innerItem, entryForItem.Key);
            }

            entryForItem.Key = newKey;
            for (int i = 0; i < entryForItem.ReferenceCount; i++)
            {
                AddInnerItemToAllOuterEntriesMatchingKey(innerItem, newKey);
                AddInnerItemToKeyLookup(innerItem, newKey);
            }
        }

        void OnInnerAdd(object sender, int index, IEnumerable<TInner> newItems)
        {
            foreach (var innerItem in newItems)
            {
                var key = _innerKeySelector(innerItem);

                AddInnerItemToKeyLookup(innerItem, key);

                AddInnerItemToAllOuterEntriesMatchingKey(innerItem, key);
                AddInnerItemEntry(innerItem, key);
            }
        }

        private void AddInnerItemToKeyLookup(TInner innerItem, TKey key)
        {
            var innersMatchingKey = _keyToInnerLookup.GetOrCreate(key, () => new List<TInner>(1));
            innersMatchingKey.Add(innerItem);
        }

        private void AddInnerItemToAllOuterEntriesMatchingKey(TInner innerItem, TKey key)
        {
            List<OuterEntry> outersMatchingKey;
            if (_keyToOuterLookup.TryGetValue(key, out outersMatchingKey))
            {
                foreach (var outerItem in outersMatchingKey)
                {
                    outerItem.MatchingInners.Add(innerItem);
                }
            }
        }

        void OnInnerRemove(object sender, int index, IEnumerable<TInner> oldItems)
        {
            foreach (var innerItem in oldItems)
            {
                InnerEntry innerEntry = _innerEntries[innerItem];
        
                RemoveInnerItemFromAllOuterEntriesMatchingKey(innerItem, innerEntry.Key);

                RemoveInnerItemFromKeyLookup(innerItem, innerEntry.Key);

                RemoveInnerItemEntry(innerItem, innerEntry);
            }
        }

        private void RemoveInnerItemEntry(TInner innerItem, InnerEntry innerEntry)
        {
            innerEntry.ReferenceCount--;

            if (innerEntry.ReferenceCount == 0)
            {
                _innerEntries.Remove(innerItem);
            }
        }

        private void RemoveInnerItemFromKeyLookup(TInner innerItem, TKey key)
        {
            var innersMatchingKey = _keyToInnerLookup[key];

            if (innersMatchingKey.Count == 1)
            {
                _keyToInnerLookup.Remove(key);
            }
            else
            {
                innersMatchingKey.Remove(innerItem);
            }
        }

        private void RemoveInnerItemFromAllOuterEntriesMatchingKey(TInner innerItem, TKey key)
        {
            List<OuterEntry> outersMatchingKey;
            if (_keyToOuterLookup.TryGetValue(key, out outersMatchingKey))
            {
                foreach (var outerItem in outersMatchingKey)
                {
                    outerItem.MatchingInners.Remove(innerItem);
                }
            }
        }

        void OnInnerReset(object sender)
        {
            _innerEntries.Clear();
            _keyToInnerLookup.Clear();
            RebuildAllInner();

            foreach (var outerEntry in _outerEntries.Values)
            {
                outerEntry.MatchingInners.Clear();
                AddAllInnersMatchingKey(outerEntry);
            }
        }

        void OnInnerMove(object sender, int oldStartingIndex, IEnumerable<TInner> oldItems, int newStartingIndex, IEnumerable<TInner> newItems)
        {
            //No need to do anything for a move
        }

        void OnInnerItemReplace(object sender, IEnumerable<TInner> oldItems, int newStartingIndex, IEnumerable<TInner> newItems)
        {
            OnInnerRemove(sender, newStartingIndex, oldItems);
            OnInnerAdd(sender, newStartingIndex, newItems);
        }
    }
}
