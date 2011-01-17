using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq.Expressions;
using ContinuousLinq.Collections;

namespace ContinuousLinq
{
    public static class ContinuousQueryExtension
    {
        #region Select

        public static ReadOnlyContinuousCollection<TResult> Select<TSource, TResult>(
            this ObservableCollection<TSource> source, Expression<Func<TSource, TResult>> selector)
        {
            return new SelectReadOnlyContinuousCollection<TSource, TResult>(source, selector);
        }

        public static ReadOnlyContinuousCollection<TResult> Select<TSource, TResult>(
            this ReadOnlyObservableCollection<TSource> source, Expression<Func<TSource, TResult>> selector)
        {
            return new SelectReadOnlyContinuousCollection<TSource, TResult>(source, selector);
        }

        public static ReadOnlyContinuousCollection<TResult> Select<TSource, TResult>(
            this ReadOnlyContinuousCollection<TSource> source, Expression<Func<TSource, TResult>> selector)
        {
            return new SelectReadOnlyContinuousCollection<TSource, TResult>(source, selector);
        }

        #endregion

        #region SelectMany

        public static ReadOnlyContinuousCollection<TResult> SelectMany<TSource, TResult>(
            this ObservableCollection<TSource> source, Expression<Func<TSource, IList<TResult>>> manySelector)
        {
            return new SelectManyReadOnlyContinuousCollection<TSource, TResult>(source, manySelector);
        }

        public static ReadOnlyContinuousCollection<TResult> SelectMany<TSource, TResult>(
            this ReadOnlyObservableCollection<TSource> source, Expression<Func<TSource, IList<TResult>>> manySelector)
        {
            return new SelectManyReadOnlyContinuousCollection<TSource, TResult>(source, manySelector);
        }

        public static ReadOnlyContinuousCollection<TResult> SelectMany<TSource, TResult>(
            this ReadOnlyContinuousCollection<TSource> source, Expression<Func<TSource, IList<TResult>>> manySelector)
        {
            return new SelectManyReadOnlyContinuousCollection<TSource, TResult>(source, manySelector);
        }

        //public static ReadOnlyContinuousCollection<TResult> SelectMany<TSource, TCollection, TResult>(
        //    this ObservableCollection<TSource> source,
        //    Expression<Func<TSource, ObservableCollection<TCollection>>> collectionSelector,
        //    Expression<Func<TSource, TCollection, TResult>> resultSelector)
        //{
        //    //return new CollectionFlatteningSelectManyReadOnlyContinuousCollection<TSource, TCollection, TResult>(source, collectionSelector, resultSelector);
        //    throw new Exception();
        //}

        #endregion

        #region GroupJoin

        public static ReadOnlyContinuousCollection<TResult> GroupJoin<TOuter, TInner, TKey, TResult>(
            this ObservableCollection<TOuter> outer,
            IList<TInner> inner,
            Expression<Func<TOuter, TKey>> outerKeySelector,
            Expression<Func<TInner, TKey>> innerKeySelector,
            Expression<Func<TOuter, ReadOnlyContinuousCollection<TInner>, TResult>> resultSelector)
        {
            return new GroupJoinReadOnlyContinuousCollection<TOuter, TInner, TKey, TResult>(
                outer,
                inner,
                outerKeySelector,
                innerKeySelector,
                resultSelector);
        }

        public static ReadOnlyContinuousCollection<TResult> GroupJoin<TOuter, TInner, TKey, TResult>(
            this ReadOnlyContinuousCollection<TOuter> outer,
            IList<TInner> inner,
            Expression<Func<TOuter, TKey>> outerKeySelector,
            Expression<Func<TInner, TKey>> innerKeySelector,
            Expression<Func<TOuter, ReadOnlyContinuousCollection<TInner>, TResult>> resultSelector)
        {
            return new GroupJoinReadOnlyContinuousCollection<TOuter, TInner, TKey, TResult>(
                outer,
                inner,
                outerKeySelector,
                innerKeySelector,
                resultSelector);
        }

        public static ReadOnlyContinuousCollection<TResult> GroupJoin<TOuter, TInner, TKey, TResult>(
            this ReadOnlyObservableCollection<TOuter> outer,
            IList<TInner> inner,
            Expression<Func<TOuter, TKey>> outerKeySelector,
            Expression<Func<TInner, TKey>> innerKeySelector,
            Expression<Func<TOuter, ReadOnlyContinuousCollection<TInner>, TResult>> resultSelector)
        {
            return new GroupJoinReadOnlyContinuousCollection<TOuter, TInner, TKey, TResult>(
                outer,
                inner,
                outerKeySelector,
                innerKeySelector,
                resultSelector);
        }
        #endregion

        #region Where

        public static ReadOnlyContinuousCollection<TSource> Where<TSource>(
            this ObservableCollection<TSource> source, Expression<Func<TSource, bool>> filter)
            where TSource : INotifyPropertyChanged
        {
            return new FilteringReadOnlyContinuousCollection<TSource>(source, filter);
        }

        public static ReadOnlyContinuousCollection<TSource> Where<TSource>(
            this ReadOnlyObservableCollection<TSource> source, Expression<Func<TSource, bool>> filter)
            where TSource : INotifyPropertyChanged
        {
            return new FilteringReadOnlyContinuousCollection<TSource>(source, filter);
        }

        public static ReadOnlyContinuousCollection<TSource> Where<TSource>(
            this ReadOnlyContinuousCollection<TSource> source, Expression<Func<TSource, bool>> filter)
            where TSource : INotifyPropertyChanged
        {
            return new FilteringReadOnlyContinuousCollection<TSource>(source, filter);
        }

        #endregion

        #region OrderBy

        public static OrderedReadOnlyContinuousCollection<TSource> OrderBy<TSource, TKey>(
            this ObservableCollection<TSource> source,
            Expression<Func<TSource, TKey>> keySelector)
            where TSource : INotifyPropertyChanged
            where TKey : IComparable
        {
            return new SortingReadOnlyContinuousCollection<TSource, TKey>(source, keySelector, false);
        }

        public static OrderedReadOnlyContinuousCollection<TSource> OrderBy<TSource, TKey>(
            this ReadOnlyObservableCollection<TSource> source,
            Expression<Func<TSource, TKey>> keySelector)
            where TSource : INotifyPropertyChanged
            where TKey : IComparable
        {
            return new SortingReadOnlyContinuousCollection<TSource, TKey>(source, keySelector, false);
        }

        public static OrderedReadOnlyContinuousCollection<TSource> OrderBy<TSource, TKey>(
            this ReadOnlyContinuousCollection<TSource> source,
            Expression<Func<TSource, TKey>> keySelector)
            where TSource : INotifyPropertyChanged
            where TKey : IComparable
        {
            return new SortingReadOnlyContinuousCollection<TSource, TKey>(source, keySelector, false);
        }

        #endregion

        #region OrderByDescending

        public static OrderedReadOnlyContinuousCollection<TSource> OrderByDescending<TSource, TKey>(
           this ObservableCollection<TSource> source,
           Expression<Func<TSource, TKey>> keySelector)
            where TSource : INotifyPropertyChanged
            where TKey : IComparable
        {
            return new SortingReadOnlyContinuousCollection<TSource, TKey>(source, keySelector, true);
        }

        public static OrderedReadOnlyContinuousCollection<TSource> OrderByDescending<TSource, TKey>(
            this ReadOnlyObservableCollection<TSource> source,
            Expression<Func<TSource, TKey>> keySelector)
            where TSource : INotifyPropertyChanged
            where TKey : IComparable
        {
            return new SortingReadOnlyContinuousCollection<TSource, TKey>(source, keySelector, true);
        }

        public static OrderedReadOnlyContinuousCollection<TSource> OrderByDescending<TSource, TKey>(
            this ReadOnlyContinuousCollection<TSource> source,
            Expression<Func<TSource, TKey>> keySelector)
            where TSource : INotifyPropertyChanged
            where TKey : IComparable
        {
            return new SortingReadOnlyContinuousCollection<TSource, TKey>(source, keySelector, true);
        }

        #endregion

        #region ThenBy

        public static ReadOnlyContinuousCollection<TSource> ThenBy<TSource, TKey>(
            this OrderedReadOnlyContinuousCollection<TSource> source,
            Expression<Func<TSource, TKey>> keySelector)
            where TSource : INotifyPropertyChanged
            where TKey : IComparable
        {
            return new ThenByReadOnlyContinuousCollection<TSource, TKey>(source, keySelector, false);
        }

        #endregion

        #region ThenByDescending

        public static ReadOnlyContinuousCollection<TSource> ThenByDescending<TSource, TKey>(
            this OrderedReadOnlyContinuousCollection<TSource> source,
            Expression<Func<TSource, TKey>> keySelector)
            where TSource : INotifyPropertyChanged
            where TKey : IComparable
        {
            return new ThenByReadOnlyContinuousCollection<TSource, TKey>(source, keySelector, true);
        }

        #endregion

        #region Distinct

        public static ReadOnlyContinuousCollection<TSource> Distinct<TSource>(
           this ObservableCollection<TSource> source)
        {
            return new DistinctReadOnlyContinuousCollection<TSource>(source);
        }

        public static ReadOnlyContinuousCollection<TSource> Distinct<TSource>(
            this ReadOnlyObservableCollection<TSource> source)
        {
            return new DistinctReadOnlyContinuousCollection<TSource>(source);
        }

        public static ReadOnlyContinuousCollection<TSource> Distinct<TSource>(
            this ReadOnlyContinuousCollection<TSource> source)
        {
            return new DistinctReadOnlyContinuousCollection<TSource>(source);
        }

        #endregion

        #region GroupBy
        public static GroupingReadOnlyContinuousCollection<TKey, TSource> GroupBy<TKey, TSource>(
            this ObservableCollection<TSource> source,
            Expression<Func<TSource, TKey>> keySelector)
            where TSource : INotifyPropertyChanged
        {
            return new GroupingReadOnlyContinuousCollection<TKey, TSource>(source, keySelector);
        }

        public static GroupingReadOnlyContinuousCollection<TKey, TSource> GroupBy<TKey, TSource>(
            this ReadOnlyObservableCollection<TSource> source,
            Expression<Func<TSource, TKey>> keySelector)
            where TSource : INotifyPropertyChanged
        {
            return new GroupingReadOnlyContinuousCollection<TKey, TSource>(source, keySelector);
        }

        public static GroupingReadOnlyContinuousCollection<TKey, TSource> GroupBy<TKey, TSource>(
            this ReadOnlyContinuousCollection<TSource> source,
            Expression<Func<TSource, TKey>> keySelector)
            where TSource : INotifyPropertyChanged
        {
            return new GroupingReadOnlyContinuousCollection<TKey, TSource>(source, keySelector);
        }
        #endregion

        #region AsReadOnly

        public static ReadOnlyContinuousCollection<TSource> AsReadOnly<TSource>(
            this ObservableCollection<TSource> source)
        {
            return new PassThroughReadOnlyContinuousCollection<TSource>(source);
        }

        public static ReadOnlyContinuousCollection<TSource> AsReadOnly<TSource>(
            this ReadOnlyObservableCollection<TSource> source)
        {
            return new PassThroughReadOnlyContinuousCollection<TSource>(source);
        }

        #endregion

        #region Except

        public static ReadOnlyContinuousCollection<TSource> Except<TSource>(this ObservableCollection<TSource> first, ObservableCollection<TSource> second)
        {
            return new ExceptReadOnlyContinuousCollection<TSource>(first, second);
        }

        public static ReadOnlyContinuousCollection<TSource> Except<TSource>(this ReadOnlyObservableCollection<TSource> first, ReadOnlyObservableCollection<TSource> second)
        {
            return new ExceptReadOnlyContinuousCollection<TSource>(first, second);
        }

        public static ReadOnlyContinuousCollection<TSource> Except<TSource>(this ReadOnlyContinuousCollection<TSource> first, ReadOnlyContinuousCollection<TSource> second)
        {
            return new ExceptReadOnlyContinuousCollection<TSource>(first, second);
        }

        #endregion

        #region Concat

        public static ReadOnlyContinuousCollection<TSource> Concat<TSource>(this ObservableCollection<TSource> first, ObservableCollection<TSource> second)
        {
            return new ConcatReadOnlyContinuousCollection<TSource>(first, second);
        }

        public static ReadOnlyContinuousCollection<TSource> Concat<TSource>(this ReadOnlyObservableCollection<TSource> first, ReadOnlyObservableCollection<TSource> second)
        {
            return new ConcatReadOnlyContinuousCollection<TSource>(first, second);
        }

        public static ReadOnlyContinuousCollection<TSource> Concat<TSource>(this ReadOnlyContinuousCollection<TSource> first, ReadOnlyContinuousCollection<TSource> second)
        {
            return new ConcatReadOnlyContinuousCollection<TSource>(first, second);
        }

        #endregion

        #region Take
        public static ReadOnlyContinuousCollection<TSource> Take<TSource>(this ObservableCollection<TSource> collection, int count)
        {
            return new TakeReadOnlyContinuousCollection<TSource>(collection, count);
        }
        public static ReadOnlyContinuousCollection<TSource> Take<TSource>(this ReadOnlyObservableCollection<TSource> collection, int count)
        {
            return new TakeReadOnlyContinuousCollection<TSource>(collection, count);
        }
        public static ReadOnlyContinuousCollection<TSource> Take<TSource>(this ReadOnlyContinuousCollection<TSource> collection, int count)
        {
            return new TakeReadOnlyContinuousCollection<TSource>(collection, count);
        }
        #endregion

        #region Skip
        public static ReadOnlyContinuousCollection<TSource> Skip<TSource>(this ObservableCollection<TSource> collection, int count)
        {
            return new SkipReadOnlyContinuousCollection<TSource>(collection, count);
        }
        public static ReadOnlyContinuousCollection<TSource> Skip<TSource>(this ReadOnlyObservableCollection<TSource> collection, int count)
        {
            return new SkipReadOnlyContinuousCollection<TSource>(collection, count);
        }
        public static ReadOnlyContinuousCollection<TSource> Skip<TSource>(this ReadOnlyContinuousCollection<TSource> collection, int count)
        {
            return new SkipReadOnlyContinuousCollection<TSource>(collection, count);
        }
        #endregion
    }
}
