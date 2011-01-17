using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Diagnostics;
using ContinuousLinq.Expressions;

namespace ContinuousLinq.Collections
{
    internal class SortingReadOnlyContinuousCollection<TSource, TKey> : 
        OrderedReadOnlyContinuousCollection<TSource>
        where TSource : INotifyPropertyChanged
        where TKey : IComparable
    {   
        internal Func<TSource, TKey> KeySelector { get; set; }
        internal List<TSource> Output { get; set; }
                  
        public SortingReadOnlyContinuousCollection(IList<TSource> list,
            Expression<Func<TSource, TKey>> keySelectorExpression,
            bool descending)
            : base(list, ExpressionPropertyAnalyzer.Analyze(keySelectorExpression))
        {
            this.KeySelector = keySelectorExpression.CachedCompile();
            this.KeySorter = new SortsSourceByKey<TSource, TKey>(this.KeySelector, descending);
            SetComparerChain(this.KeySorter);

            BuildItemsInSortOrder(this.Source);

            this.NotifyCollectionChangedMonitor.Add += OnAdd;
            this.NotifyCollectionChangedMonitor.Remove += OnRemove;
            this.NotifyCollectionChangedMonitor.Reset += OnReset;
            this.NotifyCollectionChangedMonitor.Replace += OnReplace;
            this.NotifyCollectionChangedMonitor.ItemChanged += OnItemChanged;
        }

        protected virtual void SetComparerChain(IComparer<TSource> compareFunc)
        {
            this.KeySorter = compareFunc;
        }

        public override int Count
        {
            get { return this.Output.Count; }
        }

        public override TSource this[int index]
        {
            get
            {
                return this.Output[index];
            }
            set { throw new AccessViolationException(); }
        }

        private void InsertItemInSortOrder(TSource item)
        {
            int index = this.Output.BinarySearch(item, this.KeySorter);
            if (index < 0)
            {
                index = ~index;
            }
            this.Output.Insert(index, item);
            FireAddItem(item, index);
        }


        private void RemoveItemFromOutput(TSource item)
        {
            int index = this.Output.IndexOf(item);

            this.Output.RemoveAt(index);

            FireRemoveItem(item, index);
        }

        #region NotifyCollectionChangedMonitor Event Handlers

        void OnItemChanged(object sender, INotifyPropertyChanged itemThatChanged)
        {
            TSource item = (TSource)itemThatChanged;

            RemoveItemFromOutput(item);                
            InsertItemInSortOrder(item);                
        }

        void OnAdd(object sender, int index, IEnumerable<TSource> newItems)
        {
            foreach (TSource item in newItems)
            {
                InsertItemInSortOrder(item);
            }
        }

        void OnRemove(object sender, int index, IEnumerable<TSource> oldItems)
        {
            foreach (TSource oldItem in oldItems)
            {
                RemoveItemFromOutput(oldItem);
            }
        }
        
        void OnReset(object sender)
        {
            this.Output.Clear();
            FireReset();
        }

        void OnReplace(object sender, IEnumerable<TSource> oldItems, int newStartingIndex, IEnumerable<TSource> newItems)
        {
            foreach (TSource oldItem in oldItems)
            {
                RemoveItemFromOutput(oldItem);
            }
            
            foreach (TSource newItem in newItems)
            {
                InsertItemInSortOrder(newItem);
            }
        }
        #endregion        

        private void BuildItemsInSortOrder(IEnumerable<TSource> items)
        {
            List<TSource> sortedList = new List<TSource>(this.Source);
            sortedList.Sort(this.KeySorter);

            this.Output = new List<TSource>();
            this.Output.AddRange(sortedList);
            FireReset();
        }
    }

    internal class SortsSourceByKey<TSource, TKey> : Comparer<TSource> where TKey : IComparable
    {
        private Func<TSource, TKey> _keySelector;
        private int _multiplier;

        public SortsSourceByKey(Func<TSource, TKey> keySelector, bool descending)
        {
            if (descending)
                _multiplier = -1;
            else
                _multiplier = 1;

            _keySelector = keySelector;
        }

        public override int Compare(TSource x, TSource y)
        {
            int originalCompare = Comparer<TKey>.Default.Compare(
                _keySelector(x), _keySelector(y));
            return _multiplier * originalCompare;
        }
    }
}
