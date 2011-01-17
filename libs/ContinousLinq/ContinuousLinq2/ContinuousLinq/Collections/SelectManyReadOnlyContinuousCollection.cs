using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.ComponentModel;
using System.Collections.Specialized;
using ContinuousLinq.Expressions;
using System.Collections;
using ContinuousLinq.WeakEvents;

namespace ContinuousLinq.Collections
{
    public class SelectManyReadOnlyContinuousCollection<TSource, TResult> : ReadOnlyAdapterContinuousCollection<TSource, TResult>
    {
        private Dictionary<TSource, List<IndexingSkipList<IList<TResult>>.Node>> _sourceToCollectionNode;
        private Dictionary<IList<TResult>, List<IndexingSkipList<IList<TResult>>.Node>> _collectionToNode;
        private Dictionary<IList<TResult>, WeakEventHandler> _weakEventHandlers;

        IndexingSkipList<IList<TResult>> _collectionIndex;

        private Func<TSource, IList<TResult>> _selectorFunction;

        public SelectManyReadOnlyContinuousCollection(
            IList<TSource> list,
            Expression<Func<TSource, IList<TResult>>> manySelector) :
            base(list, ExpressionPropertyAnalyzer.Analyze(manySelector))
        {
            _sourceToCollectionNode = new Dictionary<TSource, List<IndexingSkipList<IList<TResult>>.Node>>();
            _collectionToNode = new Dictionary<IList<TResult>, List<IndexingSkipList<IList<TResult>>.Node>>();
            _weakEventHandlers = new Dictionary<IList<TResult>, WeakEventHandler>();
            _collectionIndex = new IndexingSkipList<IList<TResult>>();

            _selectorFunction = manySelector.CachedCompile();
            RecordCurrentValues(_collectionIndex.TopLeft, this.Source);

            this.NotifyCollectionChangedMonitor.Add += OnAdd;
            this.NotifyCollectionChangedMonitor.Remove += OnRemove;
            this.NotifyCollectionChangedMonitor.Reset += OnReset;
            this.NotifyCollectionChangedMonitor.Move += OnMove;
            this.NotifyCollectionChangedMonitor.Replace += OnReplace;
            this.NotifyCollectionChangedMonitor.ItemChanged += OnItemChanged;
        }

        public override TResult this[int index]
        {
            get
            {
                int indexInCollection;
                var collectionContainingItem = _collectionIndex.GetLeaf(index, out indexInCollection).Value;
                return collectionContainingItem[indexInCollection];
            }
            set { throw new AccessViolationException(); }
        }

        public override int Count
        {
            get { return _collectionIndex.TotalItems; }
        }

        private void RecordCurrentValues(IndexingSkipList<IList<TResult>>.Node nodeBefore, IList<TSource> items)
        {
            IndexingSkipList<IList<TResult>>.Node lastCollectionNodeAdded = nodeBefore;
            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];
                var collection = _selectorFunction(item);

                var newNode = RecordItem(lastCollectionNodeAdded, item, collection);

                lastCollectionNodeAdded = newNode;
            }
        }

        private static int GetCount(IList<TResult> collection)
        {
            int count = collection != null ? collection.Count : 0;
            return count;
        }

        private IndexingSkipList<IList<TResult>>.Node RecordItem(IndexingSkipList<IList<TResult>>.Node nodeBefore, TSource item, IList<TResult> collection)
        {
            int count = GetCount(collection);
            var newNode = _collectionIndex.AddAfter(nodeBefore, count, collection);

            RecordNodeForItemInLookupTables(item, collection, newNode);

            if (collection != null && !_weakEventHandlers.ContainsKey(collection))
            {
                WeakEventHandler weakEventHandler = WeakNotifyCollectionChangedEventHandler.Register(
                    (INotifyCollectionChanged)collection,
                    this,
                    (me, sender, args) => me.OnSubCollectionChanged(sender, args));

                _weakEventHandlers.Add(collection, weakEventHandler);
            }

            return newNode;
        }

        private void RemoveItem(IndexingSkipList<IList<TResult>>.Node node, TSource item, IList<TResult> collection)
        {
            _collectionIndex.Remove(node);

            int numberOfInstancesOfCollectionLeft = RemoveNodeFromLookupTable(_collectionToNode, collection, node);
            RemoveNodeFromLookupTable(_sourceToCollectionNode, item, node);

            if (collection != null && numberOfInstancesOfCollectionLeft == 0)
            {
                var handler = _weakEventHandlers[collection];
                handler.Deregister();
                _weakEventHandlers.Remove(collection);
            }
        }

        private void RecordNodeForItemInLookupTables(TSource source, IList<TResult> collection, IndexingSkipList<IList<TResult>>.Node node)
        {
            AddNodeToLookupTable(_sourceToCollectionNode, source, node);
            if (collection != null)
            {
                AddNodeToLookupTable(_collectionToNode, collection, node);
            }
        }

        private static void AddNodeToLookupTable<TKey>(Dictionary<TKey, List<IndexingSkipList<IList<TResult>>.Node>> dictionary, TKey key, IndexingSkipList<IList<TResult>>.Node node)
        {
            var nodesForItem = dictionary.GetOrCreate(key, () => new List<IndexingSkipList<IList<TResult>>.Node>(1));
            nodesForItem.Add(node);
        }

        private static int RemoveNodeFromLookupTable<TKey>(Dictionary<TKey, List<IndexingSkipList<IList<TResult>>.Node>> dictionary, TKey key, IndexingSkipList<IList<TResult>>.Node node)
        {
            var nodesForItem = dictionary[key];
            nodesForItem.Remove(node);
            if (nodesForItem.Count == 0)
            {
                dictionary.Remove(key);
            }

            return nodesForItem.Count;
        }

        #region Collection Change Handlers

        public void OnSubCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            IList<TResult> collection = (IList<TResult>)sender;

            var nodesForCollection = _collectionToNode[collection];
            for (int i = 0; i < nodesForCollection.Count; i++)
            {
                _collectionIndex.UpdateItemsInNode(nodesForCollection[i], collection.Count);
            }

            if (nodesForCollection.Count == 1)
            {
                var nodeForCollection = _collectionToNode[collection][0];
                int indexOfNodeInOutput = _collectionIndex.GetIndex(nodeForCollection);
                switch (args.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        FireAddList(args.NewItems, indexOfNodeInOutput + args.NewStartingIndex);
                        break;
#if !SILVERLIGHT   
                    case NotifyCollectionChangedAction.Move:
                        FireMove(args.NewItems, indexOfNodeInOutput + args.NewStartingIndex, indexOfNodeInOutput + args.OldStartingIndex);
                        break;
#endif
                    case NotifyCollectionChangedAction.Remove:
                        FireRemoveList(args.OldItems, indexOfNodeInOutput + args.OldStartingIndex);
                        break;
                    case NotifyCollectionChangedAction.Replace:
                        FireReplaceList(args.NewItems, args.OldItems, indexOfNodeInOutput + args.NewStartingIndex);
                        break;
                    case NotifyCollectionChangedAction.Reset:
                        FireReset();
                        break;
                }
            }
            else
            {
                FireReset();
            }
        }

        void OnItemChanged(object sender, INotifyPropertyChanged itemThatChanged)
        {
            TSource item = (TSource)itemThatChanged;

            var nodesForSource = _sourceToCollectionNode[item];

            if (nodesForSource.Count > 1)
            {
                ClearAndReset();
                return;
            }

            var nodeForItem = nodesForSource[0];
            var previousNode = nodeForItem.Previous;

            var oldCollection = nodeForItem.Value;
            int indexInFlattenedCollection = _collectionIndex.GetIndex(nodeForItem);
            RemoveItem(nodeForItem, item, oldCollection);

            var newCollection = _selectorFunction(item);
            RecordItem(previousNode, item, newCollection);

            if (oldCollection != null && newCollection != null && oldCollection.Count > 0 && newCollection.Count > 0)
            {
                FireReplaceList((IList)newCollection, (IList)oldCollection, indexInFlattenedCollection);
            }
            else if (oldCollection != null && oldCollection.Count > 0)
            {
                FireRemoveList((IList)oldCollection, indexInFlattenedCollection);
            }
            else if (newCollection.Count > 0)
            {
                FireAddList((IList)newCollection, indexInFlattenedCollection);
            }
        }

        private List<TResult> AddItems(IEnumerable<TSource> newItems, IndexingSkipList<IList<TResult>>.Node previousNode)
        {
            List<TResult> newResults = new List<TResult>();

            IndexingSkipList<IList<TResult>>.Node lastCollectionNodeAdded = previousNode;

            foreach (var item in newItems)
            {
                var collection = _selectorFunction(item);

                if (collection != null)
                {
                    newResults.AddRange(collection);
                }

                var newNode = RecordItem(lastCollectionNodeAdded, item, collection);

                lastCollectionNodeAdded = newNode;
            }
            return newResults;
        }

        void OnAdd(object sender, int index, IEnumerable<TSource> newItems)
        {
            IndexingSkipList<IList<TResult>>.Node previousNode;

            if (!TryGetPreviousNode(index, out previousNode))
            {
                ClearAndReset();
                return;
            }

            List<TResult> newResults = AddItems(newItems, previousNode);

            if (newResults.Count > 0)
            {
                int startingIndexInFlattenedCollection = _collectionIndex.GetIndex(previousNode.Next);
                FireAdd(newResults, startingIndexInFlattenedCollection);
            }
        }

        private bool TryGetPreviousNode(int indexInSource, out IndexingSkipList<IList<TResult>>.Node previousNode)
        {
            previousNode = null;

            if (indexInSource > 0)
            {
                List<IndexingSkipList<IList<TResult>>.Node> nodesForSource;

                TSource previousItemInSource = this.Source[indexInSource - 1];
                nodesForSource = _sourceToCollectionNode[previousItemInSource];

                if (nodesForSource.Count > 1)
                {
                    return false;
                }

                previousNode = nodesForSource[0];
            }
            else
            {
                previousNode = _collectionIndex.BottomLeft;
            }

            return true;
        }

        private List<TResult> RemoveItems(IEnumerable<TSource> oldItems, IndexingSkipList<IList<TResult>>.Node previousNode, out int oldStartingIndex)
        {
            var oldResults = new List<TResult>();

            var firstNodeToRemove = previousNode.Next;
            var currentNode = firstNodeToRemove;

            oldStartingIndex = _collectionIndex.GetIndex(firstNodeToRemove);

            foreach (var item in oldItems)
            {
                var collection = currentNode.Value;

                oldResults.AddRange(collection);

                var nextNode = currentNode.Next;

                RemoveItem(currentNode, item, collection);

                currentNode = nextNode;
            }

            return oldResults;
        }

        void OnRemove(object sender, int index, IEnumerable<TSource> oldItems)
        {
            IndexingSkipList<IList<TResult>>.Node previousNode;

            if (!TryGetPreviousNode(index, out previousNode))
            {
                ClearAndReset();
                return;
            }

            int oldStartingIndex;
            var oldResults = RemoveItems(oldItems, previousNode, out oldStartingIndex);

            if (oldResults.Count > 0)
            {
                FireRemove(oldResults, oldStartingIndex);
            }
        }

        void OnReset(object sender)
        {
            ClearAndReset();
        }

        private void ClearAndReset()
        {
            _collectionIndex.Clear();
            ClearWeakEventHandlers();
            _collectionToNode.Clear();
            _sourceToCollectionNode.Clear();

            RecordCurrentValues(_collectionIndex.TopLeft, this.Source);
            FireReset();
        }

        private void ClearWeakEventHandlers()
        {
            foreach (var handler in _weakEventHandlers.Values)
            {
                handler.Deregister();
            }
            _weakEventHandlers.Clear();
        }

        void OnMove(object sender, int oldStartingIndex, IEnumerable<TSource> oldItems, int newStartingIndex, IEnumerable<TSource> newItems)
        {
            IndexingSkipList<IList<TResult>>.Node oldPreviousNode;

            if (!TryGetPreviousNode(oldStartingIndex, out oldPreviousNode))
            {
                ClearAndReset();
                return;
            }

            int oldStartingIndexInFlattenedCollection;
            List<TResult> oldResults = RemoveItems(oldItems, oldPreviousNode, out oldStartingIndexInFlattenedCollection);

            IndexingSkipList<IList<TResult>>.Node newPreviousNode;
            if (!TryGetPreviousNode(newStartingIndex, out newPreviousNode))
            {
                ClearAndReset();
                return;
            }

            List<TResult> newResults = AddItems(newItems, newPreviousNode);
            
            if (oldResults.Count > 0 && newResults.Count > 0)
            {
                int newStartingIndexInFlattenedCollection = _collectionIndex.GetIndex(newPreviousNode.Next);
                FireMove(newResults, newStartingIndexInFlattenedCollection, oldStartingIndexInFlattenedCollection);
            }
        }

        void OnReplace(object sender, IEnumerable<TSource> oldItems, int newStartingIndex, IEnumerable<TSource> newItems)
        {
            IndexingSkipList<IList<TResult>>.Node previousNode;

            if (!TryGetPreviousNode(newStartingIndex, out previousNode))
            {
                ClearAndReset();
                return;
            }

            int oldStartingIndexInFlattenedCollection;
            List<TResult> oldResults = RemoveItems(oldItems, previousNode, out oldStartingIndexInFlattenedCollection);
            List<TResult> newResults = AddItems(newItems, previousNode);

            if (oldResults.Count > 0 && newResults.Count > 0)
            {
                FireReplace(newResults, oldResults, oldStartingIndexInFlattenedCollection);
            }
            else if (oldResults.Count > 0)
            {
                FireRemove(oldResults, oldStartingIndexInFlattenedCollection);
            }
            else if (newResults.Count > 0)
            {
                FireAdd(newResults, oldStartingIndexInFlattenedCollection);
            }
        }

        public override IEnumerator<TResult> GetEnumerator()
        {
            foreach (var subList in _collectionIndex)
            {
                foreach (var item in subList)
                {
                    yield return item;
                }
            }
        }

        #endregion
    }
}
