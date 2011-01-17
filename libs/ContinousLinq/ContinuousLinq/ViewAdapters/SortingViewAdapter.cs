using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using ISimpleComparer = System.Collections.IComparer;
using System.Linq.Expressions;

namespace ContinuousLinq
{
    /// <summary>
    /// This adapter applies a sort clause to an input collection. The output collection represents
    /// the sorted contents of the input collection, sorted according to the sort clause indicated
    /// by the compare function. As changes to the input collection are detected (which could be caused
    /// by other adapters using that as an output collection in the chain), the output collection
    /// is re-sorted accordingly.
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    internal sealed class SortingViewAdapter<TSource,TKey> :
        SmartViewAdapter<TSource, TSource> where TSource : INotifyPropertyChanged where TKey : IComparable
    {
        private IComparer<TSource> _compareFunc;
        private bool _isLastInChain = true;

        public SortingViewAdapter(InputCollectionWrapper<TSource> input,
            LinqContinuousCollection<TSource> output, Expression<Func<TSource, TKey>> keySelectorExpr,
            bool descending)
            : base(input, output,
            ExpressionPropertyAnalyzer.GetReferencedPropertyNames(keySelectorExpr)[typeof(TSource)])
        {
            if (keySelectorExpr == null)
                throw new ArgumentNullException("keySelectorExpr");
            _compareFunc = new FuncComparer<TSource, TKey>(keySelectorExpr.Compile(), descending);

            
            SetComparerChain(_compareFunc);
            FullSort(); // Because we do not know yet if we are last in chain.
        }

        private void SetComparerChain(IComparer<TSource> compareFunc)
        {
            SortingViewAdapter<TSource,TKey> previous = this.PreviousAdapter as SortingViewAdapter<TSource,TKey>;
            if (previous != null)
            {
                previous._isLastInChain = false;
                _compareFunc = new ChainComparer(previous._compareFunc, compareFunc);
            }
            else
            {
                _compareFunc = compareFunc;
            }
        }

        private void FullSort()
        {            
            List<TSource> sortedList = new List<TSource>(this.InputCollection);
            sortedList.Sort(_compareFunc);

            this.OutputCollection.Clear();
            this.OutputCollection.AddRange(sortedList);
        }

        /// <summary>
        /// This can probably be optimized to cost O(log2N) instead of O(N)
        /// </summary>
        /// <param name="item"></param>
        private void InsertItemInSortOrder(TSource item)
        {
            int index = this.OutputCollection.BinarySearch(item, _compareFunc);
            if (index < 0)
            {
                index = ~index;
            }
            
            this.OutputCollection.Insert(index, item);
        }


        #region Smart View Adapter Overrides
        protected override void ItemAdded(TSource item)
        {
            if (_isLastInChain)
            {
                InsertItemInSortOrder(item);
            }
            else
            {
                this.OutputCollection.Add(item);
            }
        }

        protected override void ItemPropertyChanged(TSource item)
        {
            if (_isLastInChain)
            {
                if (this.OutputCollection.Remove(item))
                {
                    InsertItemInSortOrder(item);
                }
                // Else, already deleted.
            }
        }

        protected override bool ItemRemoved(TSource item)
        {
            return this.OutputCollection.Remove(item);
        }

        protected override void ItemsCleared()
        {
            this.OutputCollection.Clear();
        }
        #endregion

        private class ChainComparer : IComparer<TSource>
        {
            private readonly IComparer<TSource> _previousComparer;
            private readonly IComparer<TSource> _currentComparer;

            public ChainComparer(IComparer<TSource> previousComparer, IComparer<TSource> currentComparer)
            {
                _previousComparer = previousComparer;
                _currentComparer = currentComparer;
            }

            public int Compare(TSource x, TSource y)
            {
                int result = _previousComparer.Compare(x, y);
                if (result != 0)
                    return result;
                return _currentComparer.Compare(x, y);
            }
        }


        public override void ReEvaluate()
        {
            if (_isLastInChain)
            {
                FullSort();
            }
        }
    }
}
