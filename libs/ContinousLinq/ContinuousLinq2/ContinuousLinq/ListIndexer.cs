using System;
using System.Collections.Generic;

namespace ContinuousLinq
{
    public class ListIndexer<TSource>
    {
        private IList<TSource> Source { get; set; }

        private Dictionary<TSource, List<IndexingSkipList<TSource>.Node>> CurrentIndices { get; set; }
        private IndexingSkipList<TSource> _index;

        public IEnumerable<int> this[TSource item]
        {
            get
            {
                foreach (var node in this.CurrentIndices[item])
                {
                    yield return _index.GetIndex(node);
                }
            }
        }

        public ListIndexer(IList<TSource> source)
        {
            this.Source = source;
            this.CurrentIndices = new Dictionary<TSource, List<IndexingSkipList<TSource>.Node>>(this.Source.Count);
            _index = new IndexingSkipList<TSource>();
            RecordIndicesOfItemsInSource();
        }

        #region Methods

        public bool Contains(TSource item)
        {
            return this.CurrentIndices.ContainsKey(item);
        }


        private void RecordIndicesOfItemsInSource()
        {
            var currentNode = _index.BottomLeft;
            foreach (var item in this.Source)
            {
                currentNode = _index.AddAfter(currentNode, 1, item);
                var nodesForItem = GetIndices(item);
                nodesForItem.Add(currentNode);
            }
        }

        private List<IndexingSkipList<TSource>.Node> GetIndices(TSource itemInSource)
        {
            var indicesForItem = this.CurrentIndices.GetOrCreate(itemInSource, () => new List<IndexingSkipList<TSource>.Node>(1));
            return indicesForItem;
        }

        public int NumberOfIndicesForItem(TSource itemInSource)
        {
            List<IndexingSkipList<TSource>.Node> indicesForItem;
            if (this.CurrentIndices.TryGetValue(itemInSource, out indicesForItem))
            {
                return indicesForItem.Count;
            }

            return 0;
        }

        public void Add(int index, IEnumerable<TSource> newItems)
        {
            foreach (TSource item in newItems)
            {
                var node = _index.Add(index, 1, item);
                var nodesForItem = GetIndices(item);
                nodesForItem.Add(node);
            }
        }

        public void Remove(int index, IEnumerable<TSource> oldItems)
        {
            var currentNode = _index.GetLeaf(index);

            foreach (TSource item in oldItems)
            {
                var nodesForItem = this.CurrentIndices[item];
                if (nodesForItem.Count == 1)
                {
                    this.CurrentIndices.Remove(item);
                }
                else
                {
                    nodesForItem.Remove(currentNode);
                }

                var nextNode = currentNode.Next;

                _index.Remove(currentNode);

                currentNode = nextNode;
            }
        }

        public void Reset()
        {
            _index.Clear();
            this.CurrentIndices.Clear();
            RecordIndicesOfItemsInSource();
        }

        public void Replace(int index, IEnumerable<TSource> oldItems, IEnumerable<TSource> newItems)
        {
            Remove(index, oldItems);
            Add(index, newItems);
        }

        public void Move(int oldStartingIndex, IEnumerable<TSource> items, int newStartingIndex)
        {
            Remove(oldStartingIndex, items);
            Add(newStartingIndex, items);
        }

        #endregion
    }


    //public class ListIndexer<TSource>
    //{
    //    #region Constructor

    //    public ListIndexer(IList<TSource> source)
    //    {
    //        this.Source = source;
    //        this.CurrentIndices = new Dictionary<TSource, HashSet<int>>(this.Source.Count);
    //        RecordIndicesOfItemsInSource();
    //    }

    //    #endregion

    //    #region Properties

    //    private IList<TSource> Source { get; set; }

    //    private Dictionary<TSource, HashSet<int>> CurrentIndices { get; set; }

    //    public HashSet<int> this[TSource item]
    //    {
    //        get { return this.CurrentIndices[item]; }
    //    }

    //    #endregion

    //    #region Methods

    //    public bool Contains(TSource item)
    //    {
    //        return this.CurrentIndices.ContainsKey(item);
    //    }

    //    private void RecordIndicesOfItemsInSource()
    //    {
    //        for (int i = 0; i < this.Source.Count; i++)
    //        {
    //            RecordIndexOfItem(i);
    //        }
    //    }

    //    private void RecordIndexOfItem(int i)
    //    {
    //        TSource itemInSource = this.Source[i];

    //        HashSet<int> currentIndices = GetIndices(itemInSource);
    //        currentIndices.Add(i);
    //    }

    //    private HashSet<int> GetIndices(TSource itemInSource)
    //    {
    //        HashSet<int> currentIndices;
    //        if (!this.CurrentIndices.TryGetValue(itemInSource, out currentIndices))
    //        {
    //            currentIndices = new HashSet<int>();
    //            this.CurrentIndices[itemInSource] = currentIndices;
    //        }
    //        return currentIndices;
    //    }

    //    public void Add(int index, IEnumerable<TSource> newItems)
    //    {
    //        int numberOfNewItems = 0;
    //        foreach (TSource item in newItems)
    //        {
    //            var indices = GetIndices(item);
    //            indices.Add(index++);
    //            numberOfNewItems++;
    //        }

    //        int previousIndexOfCurrentItem = index - numberOfNewItems;
    //        ReindexRestOfList(index, previousIndexOfCurrentItem);
    //    }

    //    private void ReindexRestOfList(int index, int previousIndexOfCurrentItem)
    //    {
    //        for (; index < this.Source.Count; index++, previousIndexOfCurrentItem++)
    //        {
    //            UpdateIndex(index, previousIndexOfCurrentItem);
    //        }
    //    }

    //    private void UpdateIndex(int newIndex, int previousIndex)
    //    {
    //        HashSet<int> indices = this.CurrentIndices[this.Source[newIndex]];
    //        if (previousIndex >= this.Source.Count ||
    //            !EqualityComparer<TSource>.Default.Equals(this.Source[newIndex], this.Source[previousIndex]))
    //        {
    //            indices.Remove(previousIndex);
    //        }
    //        indices.Add(newIndex);
    //    }

    //    public void Remove(int index, IEnumerable<TSource> oldItems)
    //    {
    //        int numberOfOldItems = 0;
    //        int oldIndex = index;
    //        foreach (TSource item in oldItems)
    //        {
    //            RemoveFromIndexTable(item, oldIndex++);
    //            numberOfOldItems++;
    //        }

    //        int previousIndexOfCurrentItem = index + numberOfOldItems;
    //        ReindexRestOfList(index, previousIndexOfCurrentItem);
    //    }

    //    public void Reset()
    //    {
    //        this.CurrentIndices.Clear();
    //        RecordIndicesOfItemsInSource();
    //    }

    //    private void RemoveFromIndexTable(TSource item, int indexToRemove)
    //    {
    //        var indices = GetIndices(item);
    //        if (indices.Count <= 1)
    //        {
    //            this.CurrentIndices.Remove(item);
    //        }
    //        else
    //        {
    //            indices.Remove(indexToRemove);
    //        }
    //    }

    //    public void Replace(int index, IEnumerable<TSource> oldItems, IEnumerable<TSource> newItems)
    //    {
    //        int oldIndex = index;
    //        foreach (TSource item in oldItems)
    //        {
    //            RemoveFromIndexTable(item, oldIndex++);
    //        }

    //        foreach (TSource item in newItems)
    //        {
    //            var indices = GetIndices(item);
    //            indices.Add(index++);
    //        }
    //    }

    //    public void Move(int oldStartingIndex, IEnumerable<TSource> items, int newStartingIndex)
    //    {
    //        UpdateIndex(newStartingIndex, oldStartingIndex);
    //        int firstIndexToStartUpdating = Math.Min(oldStartingIndex, newStartingIndex);
    //        int lastIndexToUpdate = Math.Max(oldStartingIndex, newStartingIndex);

    //        int incrementerToFindOldIndex = Math.Sign(newStartingIndex - oldStartingIndex);
    //        for (int i = firstIndexToStartUpdating; i <= lastIndexToUpdate; i++)
    //        {
    //            if (i == newStartingIndex)
    //                continue;

    //            UpdateIndex(i, i + incrementerToFindOldIndex);
    //        }
    //    }

    //    #endregion
    //}
}
