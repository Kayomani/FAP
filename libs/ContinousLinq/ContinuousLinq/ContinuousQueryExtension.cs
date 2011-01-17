using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
//using System.Windows;
using System.Linq.Expressions;

namespace ContinuousLinq
{
    /// <summary>
    /// This class and the adapter model was inspired by the SLINQ (http://www.codeplex.com/slinq) project
    /// The philosophical difference between this project and SLINQ and others like it is that the goal of 
    /// ContinuousLINQ is to support data binding to a continually changing result set based on a query that
    /// is continuously re-evaluated as changes occur in the original collection. Other libraries for continuous
    /// queries require the UI to poll the changing collection on a timer, which isn't as continuous as I like.
    /// </summary>
    public static class ContinuousQueryExtension
    {
        #region Where
        
        private static ReadOnlyContinuousCollection<T> Where<T>(
            InputCollectionWrapper<T> source, Expression<Func<T, bool>> filterExpr) where T : INotifyPropertyChanged
        {            
            LinqContinuousCollection<T> output = new LinqContinuousCollection<T>();
            new SmartFilterAdapter<T>(source, output, filterExpr);
            return output;            
        }        
        
        public static ReadOnlyContinuousCollection<T> Where<T>(
            this ReadOnlyObservableCollection<T> source, Expression<Func<T, bool>> filterFunc) where T :  INotifyPropertyChanged
        {
            return Where(new InputCollectionWrapper<T>(source), filterFunc);
        }

        public static ReadOnlyContinuousCollection<T> Where<T>(
            this ObservableCollection<T> source,
            Expression<Func<T, bool>> filterExpr) where T : INotifyPropertyChanged
        {         
            return Where(new InputCollectionWrapper<T>(source), filterExpr);
        }
        #endregion

        #region OrderBy

        private static ReadOnlyContinuousCollection<TSource> OrderBy<TSource, TKey>(
            InputCollectionWrapper<TSource> source, Expression<Func<TSource, TKey>> keySelector)
            where TSource : INotifyPropertyChanged
            where TKey : IComparable
        {            
            LinqContinuousCollection<TSource> output = new LinqContinuousCollection<TSource>();
            new SortingViewAdapter<TSource, TKey>(
                source, output,
                keySelector, false);

            return output;             
        }

        public static ReadOnlyContinuousCollection<TSource> OrderBy<TSource, TKey>(
            this ObservableCollection<TSource> source, Expression<Func<TSource, TKey>> keySelector)
            where TSource : INotifyPropertyChanged
            where TKey : IComparable
        {
            return OrderBy(new InputCollectionWrapper<TSource>(source), keySelector);
        }
        
        public static ReadOnlyContinuousCollection<TSource> OrderBy<TSource, TKey>(
            this ReadOnlyObservableCollection<TSource> source, Expression<Func<TSource, TKey>> keySelector)
            where TSource : INotifyPropertyChanged
            where TKey : IComparable
        {
            return OrderBy(new InputCollectionWrapper<TSource>(source), keySelector);
        }
        
        public static ReadOnlyContinuousCollection<TSource> ThenBy<TSource, TKey>(
            this ObservableCollection<TSource> source, Expression<Func<TSource, TKey>> keySelector)
            where TSource : INotifyPropertyChanged
            where TKey : IComparable
        {
            return OrderBy(source, keySelector);
        }
        
        public static ReadOnlyContinuousCollection<TSource> ThenBy<TSource, TKey>(
            this ReadOnlyObservableCollection<TSource> source, Expression<Func<TSource, TKey>> keySelector)
            where TSource : INotifyPropertyChanged
            where TKey : IComparable
        {
            return OrderBy(source, keySelector);
        }
        
        #endregion

        #region OrderByDescending
        
        private static ReadOnlyContinuousCollection<TSource> OrderByDescending<TSource, TKey>(
            InputCollectionWrapper<TSource> source, Expression<Func<TSource, TKey>> keySelector)
            where TSource :  INotifyPropertyChanged
            where TKey : IComparable
        {            
            LinqContinuousCollection<TSource> output = new LinqContinuousCollection<TSource>();
            new SortingViewAdapter<TSource,TKey>(
                source, output,
                keySelector, true);
            return output;             
        }

        public static ReadOnlyContinuousCollection<TSource> OrderByDescending<TSource, TKey>(
            this ObservableCollection<TSource> source, Expression<Func<TSource, TKey>> keySelector)
            where TSource : INotifyPropertyChanged
            where TKey : IComparable
        {
            return OrderByDescending(new InputCollectionWrapper<TSource>(source), keySelector);
        }
        
        public static ReadOnlyContinuousCollection<TSource> OrderByDescending<TSource, TKey>(
            this ReadOnlyObservableCollection<TSource> source, Expression<Func<TSource, TKey>> keySelector)
            where TSource :  INotifyPropertyChanged
            where TKey : IComparable
        {
            return OrderByDescending(new InputCollectionWrapper<TSource>(source), keySelector);
        }
        
        public static ReadOnlyContinuousCollection<TSource> ThenByDescending<TSource, TKey>(
            this ObservableCollection<TSource> source, Expression<Func<TSource, TKey>> keySelector)
            where TSource :  INotifyPropertyChanged
            where TKey : IComparable
        {
            return OrderByDescending(source, keySelector);
        }
        
        public static ReadOnlyContinuousCollection<TSource> ThenByDescending<TSource, TKey>(
            this ReadOnlyObservableCollection<TSource> source, Expression<Func<TSource, TKey>> keySelector)
            where TSource :  INotifyPropertyChanged
            where TKey : IComparable
        {
            return OrderByDescending(source, keySelector);
        }
        
        #endregion

        #region GroupBy

        private static ReadOnlyContinuousCollection<GroupedContinuousCollection<TKey, TElement>> GroupBy<TSource, TKey, TElement>(
            InputCollectionWrapper<TSource> source,
            Expression<Func<TSource, TKey>> keySelector,
            Expression<Func<TSource, TElement>> elementSelector,
            IEqualityComparer<TKey> comparer)
            where TSource :  INotifyPropertyChanged
            where TKey: IComparable            
        {
            LinqContinuousCollection<GroupedContinuousCollection<TKey, TElement>> output =
                new LinqContinuousCollection<GroupedContinuousCollection<TKey, TElement>>();

            new GroupingViewAdapter<TSource, TKey, TElement>(source, output, keySelector, elementSelector, comparer);
            return output;
        }

        /*private static T IdentitySelector<T>(T input)
        {
            return input;
        }

        private static Expression<Func<TSource, TSource>> IdentitySelector<TSource>(TSource input)
        {            
            ParameterExpression pe = Expression.Parameter(typeof(TSource), "element");
            ConstantExpression ce = Expression.Constant(input, typeof(TSource));                        
            Expression<Func<TSource, TSource>> lambda =
                Expression.Lambda<Func<TSource, TSource>>(
                ce);

            return lambda;
        } */

        public static ReadOnlyContinuousCollection<GroupedContinuousCollection<TKey, TSource>> GroupBy<TSource, TKey>(
            this ObservableCollection<TSource> source,
            Expression<Func<TSource, TKey>> keySelector)
            where TSource : INotifyPropertyChanged
            where TKey : IComparable
        {
            return GroupBy(source, keySelector, null);
        }

        public static ReadOnlyContinuousCollection<GroupedContinuousCollection<TKey, TSource>> GroupBy<TSource, TKey>(
            this ReadOnlyObservableCollection<TSource> source,
            Expression<Func<TSource, TKey>> keySelector)
            where TSource : INotifyPropertyChanged
            where TKey : IComparable
        {
            return GroupBy(source, keySelector, null);
        }

        public static ReadOnlyContinuousCollection<GroupedContinuousCollection<TKey, TSource>> GroupBy<TSource, TKey>(
            this ObservableCollection<TSource> source,
            Expression<Func<TSource, TKey>> keySelector,
            IEqualityComparer<TKey> comparer)
            where TSource : INotifyPropertyChanged
            where TKey : IComparable
        {
            Expression<Func<TSource, TSource>> elementSelector = s => s;
            return GroupBy(source, keySelector, elementSelector, comparer);
        }

        public static ReadOnlyContinuousCollection<GroupedContinuousCollection<TKey, TSource>> GroupBy<TSource, TKey>(
            this ReadOnlyObservableCollection<TSource> source,
            Expression<Func<TSource, TKey>> keySelector,
            IEqualityComparer<TKey> comparer)
            where TSource :  INotifyPropertyChanged
            where TKey : IComparable
        {
            Expression<Func<TSource, TSource>> elementSelector = s => s;
            return GroupBy(source, keySelector, elementSelector, comparer);
        }

        public static ReadOnlyContinuousCollection<GroupedContinuousCollection<TKey, TElement>> GroupBy<TSource, TKey, TElement>(
            this ObservableCollection<TSource> source,
            Expression<Func<TSource, TKey>> keySelector,
            Expression<Func<TSource, TElement>> elementSelector)
            where TSource :  INotifyPropertyChanged
            where TKey : IComparable
        {
            return GroupBy(source, keySelector, elementSelector, null);
        }

        public static ReadOnlyContinuousCollection<GroupedContinuousCollection<TKey, TElement>> GroupBy<TSource, TKey, TElement>(
            this ReadOnlyObservableCollection<TSource> source,
            Expression<Func<TSource, TKey>> keySelector,
            Expression<Func<TSource, TElement>> elementSelector)
            where TSource :  INotifyPropertyChanged
            where TKey : IComparable
        {
            return GroupBy(source, keySelector, elementSelector, null);
        }

        public static ReadOnlyContinuousCollection<GroupedContinuousCollection<TKey, TElement>> GroupBy<TSource, TKey, TElement>(
            this ObservableCollection<TSource> source,
            Expression<Func<TSource, TKey>> keySelector,
            Expression<Func<TSource, TElement>> elementSelector,
            IEqualityComparer<TKey> comparer)
            where TSource : INotifyPropertyChanged
            where TKey : IComparable
        {
            return GroupBy(new InputCollectionWrapper<TSource>(source), keySelector, elementSelector, comparer);
        }

        public static ReadOnlyContinuousCollection<GroupedContinuousCollection<TKey, TElement>> GroupBy<TSource, TKey, TElement>(
            this ReadOnlyObservableCollection<TSource> source,
            Expression<Func<TSource, TKey>> keySelector,
            Expression<Func<TSource, TElement>> elementSelector,
            IEqualityComparer<TKey> comparer)
            where TSource :  INotifyPropertyChanged
            where TKey : IComparable
        {
            return GroupBy(new InputCollectionWrapper<TSource>(source), keySelector, elementSelector, comparer);
        }
        #endregion

        #region Select

        private static ReadOnlyContinuousCollection<TResult> Select<TSource, TResult>(
            InputCollectionWrapper<TSource> source, Func<TSource, TResult> selector)
            where TSource :  INotifyPropertyChanged
        {
            LinqContinuousCollection<TResult> output = new LinqContinuousCollection<TResult>();
            new SelectingViewAdapter<TSource, TResult>(source, output, selector);

            return output;
        }

        public static ReadOnlyContinuousCollection<TResult> Select<TSource, TResult>(
            this ObservableCollection<TSource> source, Func<TSource, TResult> selector)
            where TSource :  INotifyPropertyChanged
        {
            return Select(new InputCollectionWrapper<TSource>(source), selector);
        }

        public static ReadOnlyContinuousCollection<TResult> Select<TSource, TResult>(
            this ReadOnlyObservableCollection<TSource> source, Func<TSource, TResult> selector)
            where TSource : INotifyPropertyChanged
        {
            return Select(new InputCollectionWrapper<TSource>(source), selector);
        }

        #endregion

        #region SelectMany

        private static ReadOnlyContinuousCollection<TResult> SelectMany<TSource, TResult>(
            InputCollectionWrapper<TSource> source, Func<TSource, IEnumerable<TResult>> selector)
            where TSource :  INotifyPropertyChanged
        {
            LinqContinuousCollection<TResult> output = new LinqContinuousCollection<TResult>();
            new SelectingManyViewAdapter<TSource, TResult>(source, output, selector);

            return output;
        }

        public static ReadOnlyContinuousCollection<TResult> SelectMany<TSource, TResult>(
            this ObservableCollection<TSource> source, Func<TSource, IEnumerable<TResult>> selector)
            where TSource : INotifyPropertyChanged
        {
            return SelectMany(new InputCollectionWrapper<TSource>(source), selector);
        }

        public static ReadOnlyContinuousCollection<TResult> SelectMany<TSource, TResult>(
            this ReadOnlyObservableCollection<TSource> source, Func<TSource, IEnumerable<TResult>> selector)
            where TSource :  INotifyPropertyChanged
        {
            return SelectMany(new InputCollectionWrapper<TSource>(source), selector);
        }

        #endregion

        #region Distinct

        /// <summary>
        /// Private static method for Distinct operating using the 
        /// default IEqualityComparer of T.
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        private static ReadOnlyContinuousCollection<TSource> Distinct<TSource>(
            InputCollectionWrapper<TSource> source)
            where TSource : INotifyPropertyChanged
        {
            LinqContinuousCollection<TSource> output = new LinqContinuousCollection<TSource>();
            new DistinctViewAdapter<TSource>(source, output);

            return output;
        }

        private static ReadOnlyContinuousCollection<TSource> Distinct<TSource>(
            InputCollectionWrapper<TSource> source,
            IEqualityComparer<TSource> comparer)
            where TSource:  INotifyPropertyChanged
        {
            LinqContinuousCollection<TSource> output = new LinqContinuousCollection<TSource>();
            new DistinctViewAdapter<TSource>(source, output, comparer);

            return output;
        }

        /// <summary>
        /// Public static method for Distinct extension using default
        /// IEqualityComparer of T.
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static ReadOnlyContinuousCollection<TSource> Distinct<TSource>(
            this ObservableCollection<TSource> source)
            where TSource : INotifyPropertyChanged
        {
            return Distinct(new InputCollectionWrapper<TSource>(source));
        }

        /// <summary>
        /// Public static method for Distinct extension using default
        /// IEqualityComparer of T.
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <param name="comparer"></param>
        /// <returns></returns>
        public static ReadOnlyContinuousCollection<TSource> Distinct<TSource>(
            this ReadOnlyObservableCollection<TSource> source,
            IEqualityComparer<TSource> comparer)
            where TSource : INotifyPropertyChanged
        {
            return Distinct(new InputCollectionWrapper<TSource>(source), comparer);
        }

        public static ReadOnlyContinuousCollection<TSource> Distinct<TSource>(
            this ObservableCollection<TSource> source,
            IEqualityComparer<TSource> comparer)
            where TSource : INotifyPropertyChanged
        {
            return Distinct(new InputCollectionWrapper<TSource>(source), comparer);
        }
        #endregion

        #region Except        
        
        private static ReadOnlyContinuousCollection<TSource> Except<TSource>(
            InputCollectionWrapper<TSource> input1,
            InputCollectionWrapper<TSource> input2)            
        {
            var output = new LinqContinuousCollection<TSource>();
            new ExceptViewAdapter<TSource>(input1, input2, output);
            return output;
        }  

        public static ReadOnlyContinuousCollection<TSource> Except<TSource>(
            this ObservableCollection<TSource> input1,
            ReadOnlyObservableCollection<TSource> input2)            
        {
            return Except(new InputCollectionWrapper<TSource>(input1), new InputCollectionWrapper<TSource>(input2));
        }

        public static ReadOnlyContinuousCollection<TSource> Except<TSource>(
            this ReadOnlyObservableCollection<TSource> input1,
            ReadOnlyObservableCollection<TSource> input2)            
        {
            return Except(new InputCollectionWrapper<TSource>(input1), new InputCollectionWrapper<TSource>(input2));
        }

        public static ReadOnlyContinuousCollection<TSource> Except<TSource>(
            this ObservableCollection<TSource> input1,
            ObservableCollection<TSource> input2)            
        {
            return Except(new InputCollectionWrapper<TSource>(input1), new InputCollectionWrapper<TSource>(input2));
        }

        public static ReadOnlyContinuousCollection<TSource> Except<TSource>(
            this ReadOnlyObservableCollection<TSource> input1,
            ObservableCollection<TSource> input2)            
        {
            return Except(new InputCollectionWrapper<TSource>(input1), new InputCollectionWrapper<TSource>(input2));
        }

        #endregion

        #region Helpers

        /// <summary>
        /// When a change occurs, which is not a property change on a collection item,
        /// but changes the result of a Where() filter
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        public static void ReEvaluate<TSource>(this ReadOnlyContinuousCollection<TSource> source)
        {
            LinqContinuousCollection<TSource> collection = source as LinqContinuousCollection<TSource>;
            if (collection == null)
                return;
            List<IViewAdapter> adapterCollection = new List<IViewAdapter>();
            IViewAdapter adapter = collection.SourceAdapter;
            while (adapter != null)
            {
                adapterCollection.Add(adapter);
                adapter = adapter.PreviousAdapter;
            }
            adapterCollection.Reverse();
            adapterCollection.ForEach(a => a.ReEvaluate());
        }

        #endregion
    }
}
