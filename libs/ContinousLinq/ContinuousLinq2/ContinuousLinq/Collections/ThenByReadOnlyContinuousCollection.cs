using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Diagnostics;

namespace ContinuousLinq.Collections
{
    internal class ThenByReadOnlyContinuousCollection<TSource, TKey> :
            SortingReadOnlyContinuousCollection<TSource, TKey>
        where TSource : INotifyPropertyChanged
        where TKey : IComparable
    {
        public ThenByReadOnlyContinuousCollection(OrderedReadOnlyContinuousCollection<TSource> list,
            Expression<Func<TSource, TKey>> keySelectorExpression,
            bool descending)
            : base(list, keySelectorExpression, descending)
        {
        }

        protected override void SetComparerChain(IComparer<TSource> compareFunc)
        {
            OrderedReadOnlyContinuousCollection<TSource> previous = this.Source as OrderedReadOnlyContinuousCollection<TSource>;
            if (previous != null)
            {
                this.KeySorter = new ChainComparer(previous.KeySorter, compareFunc);
            }
        }

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
    }
}
