using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using ContinuousLinq.Expressions;

namespace ContinuousLinq.Aggregates
{
    public static class ContinuousAggregationExtension
    {        
        #region SUM

        public static ContinuousValue<int> ContinuousSum<T>(
            this ObservableCollection<T> input,
            Expression<Func<T, int>> sumFunc) where T : INotifyPropertyChanged
        {
            return new OnlineCalculationContinuousValue<T, int, int>(
                input, 
                sumFunc,
                (itemValue, oldCV) => oldCV + itemValue,
                (itemValue, oldCV) => oldCV - itemValue,
                (oldItemValue, newItemValue, oldCV) => oldCV - oldItemValue + newItemValue, 
                null);
        }
        
        //double's can not be online'd because of precision float
        public static ContinuousValue<double> ContinuousSum<T>(
            this ObservableCollection<T> input,
            Expression<Func<T, double>> sumFunc) where T : INotifyPropertyChanged
        {
            return new ContinuousValue<T, double, double>(input, sumFunc, (list, selector) => list.Sum(selector));
        }

        public static ContinuousValue<decimal> ContinuousSum<T>(
            this ObservableCollection<T> input,
            Expression<Func<T, decimal>> sumFunc) where T : INotifyPropertyChanged
        {
            return new OnlineCalculationContinuousValue<T, decimal, decimal>(
                input,
                sumFunc,
                (itemValue, oldCV) => oldCV + itemValue,
                (itemValue, oldCV) => oldCV - itemValue,
                (oldItemValue, newItemValue, oldCV) => oldCV - oldItemValue + newItemValue,    
                null);
        }

        public static ContinuousValue<float> ContinuousSum<T>(
            this ObservableCollection<T> input,
            Expression<Func<T, float>> sumFunc) where T : INotifyPropertyChanged
        {
            return new ContinuousValue<T, float, float>(input, sumFunc, (list, selector) => list.Sum(selector));
        }

        public static ContinuousValue<long> ContinuousSum<T>(
            this ObservableCollection<T> input,
            Expression<Func<T, long>> sumFunc) where T : INotifyPropertyChanged
        {
            return new OnlineCalculationContinuousValue<T, long, long>(
                input,
                sumFunc,
                (itemValue, oldCV) => oldCV + itemValue,
                (itemValue, oldCV) => oldCV - itemValue,
                (oldItemValue, newItemValue, oldCV) => oldCV - oldItemValue + newItemValue,
                null);

        }

        public static ContinuousValue<int> ContinuousSum<T>(
            this ReadOnlyObservableCollection<T> input,
            Expression<Func<T, int>> sumFunc) where T : INotifyPropertyChanged
        {
            return new OnlineCalculationContinuousValue<T, int, int>(
                input,
                sumFunc,
                (itemValue, oldCV) => oldCV + itemValue,
                (itemValue, oldCV) => oldCV - itemValue,
                (oldItemValue, newItemValue, oldCV) => oldCV - oldItemValue + newItemValue,
                null);

        }

        public static ContinuousValue<double> ContinuousSum<T>(
            this ReadOnlyObservableCollection<T> input,
            Expression<Func<T, double>> sumFunc) where T : INotifyPropertyChanged
        {
            return new ContinuousValue<T, double, double>(input, sumFunc, (list, selector) => list.Sum(selector));
        }

        public static ContinuousValue<decimal> ContinuousSum<T>(
            this ReadOnlyObservableCollection<T> input,
            Expression<Func<T, decimal>> sumFunc) where T : INotifyPropertyChanged
        {
            return new OnlineCalculationContinuousValue<T, decimal, decimal>(
                input,
                sumFunc,
                (itemValue, oldCV) => oldCV + itemValue,
                (itemValue, oldCV) => oldCV - itemValue,
                (oldItemValue, newItemValue, oldCV) => oldCV - oldItemValue + newItemValue,
                null);
        }

        public static ContinuousValue<float> ContinuousSum<T>(
            this ReadOnlyObservableCollection<T> input,
            Expression<Func<T, float>> sumFunc) where T : INotifyPropertyChanged
        {
            return new ContinuousValue<T, float, float>(input, sumFunc, (list, selector) => list.Sum(selector));
        }

        public static ContinuousValue<long> ContinuousSum<T>(
            this ReadOnlyObservableCollection<T> input,
            Expression<Func<T, long>> sumFunc) where T : INotifyPropertyChanged
        {
            return new OnlineCalculationContinuousValue<T, long, long>(
                input,
                sumFunc,
                (itemValue, oldCV) => oldCV + itemValue,
                (itemValue, oldCV) => oldCV - itemValue,
                (oldItemValue, newItemValue, oldCV) => oldCV - oldItemValue + newItemValue,
                null);

        }

        public static ContinuousValue<int> ContinuousSum<T>(
            this ReadOnlyContinuousCollection<T> input,
            Expression<Func<T, int>> sumFunc) where T : INotifyPropertyChanged
        {
            return new OnlineCalculationContinuousValue<T, int, int>(
                input,
                sumFunc,
                (itemValue, oldCV) => oldCV + itemValue,
                (itemValue, oldCV) => oldCV - itemValue,
                (oldItemValue, newItemValue, oldCV) => oldCV - oldItemValue + newItemValue,
                null);
        }

        public static ContinuousValue<double> ContinuousSum<T>(
            this ReadOnlyContinuousCollection<T> input,
            Expression<Func<T, double>> sumFunc) where T : INotifyPropertyChanged
        {
            return new ContinuousValue<T, double, double>(input, sumFunc, (list, selector) => list.Sum(selector));
        }

        public static ContinuousValue<decimal> ContinuousSum<T>(
            this ReadOnlyContinuousCollection<T> input,
            Expression<Func<T, decimal>> sumFunc) where T : INotifyPropertyChanged
        {
            return new OnlineCalculationContinuousValue<T, decimal, decimal>(
                input,
                sumFunc,
                (itemValue, oldCV) => oldCV + itemValue,
                (itemValue, oldCV) => oldCV - itemValue,
                (oldItemValue, newItemValue, oldCV) => oldCV - oldItemValue + newItemValue,
                null);
        }

        public static ContinuousValue<float> ContinuousSum<T>(
            this ReadOnlyContinuousCollection<T> input,
            Expression<Func<T, float>> sumFunc) where T : INotifyPropertyChanged
        {
            return new ContinuousValue<T, float, float>(input, sumFunc, (list, selector) => list.Sum(selector));
        }

        public static ContinuousValue<long> ContinuousSum<T>(
            this ReadOnlyContinuousCollection<T> input,
            Expression<Func<T, long>> sumFunc) where T : INotifyPropertyChanged
        {
            return new OnlineCalculationContinuousValue<T, long, long>(
                input,
                sumFunc,
                (itemValue, oldCV) => oldCV + itemValue,
                (itemValue, oldCV) => oldCV - itemValue,
                (oldItemValue, newItemValue, oldCV) => oldCV - oldItemValue + newItemValue,
                null);
        }

        public static ContinuousValue<double?> ContinuousSum<T>(
           this ReadOnlyContinuousCollection<T> input,
           Expression<Func<T, double?>> sumFunc) where T : INotifyPropertyChanged
        {
            return new ContinuousValue<T, double?, double?>(input, sumFunc, (list, selector) => list.SumNullable(selector));
        }
        public static ContinuousValue<double?> ContinuousSum<T>(
            this ReadOnlyContinuousCollection<T> input,
            Expression<Func<T, double?>> sumFunc,
            Action<double?> afterEffect) where T : INotifyPropertyChanged
        {
            return new ContinuousValue<T, double?, double?>(input, sumFunc, (list, selector) => list.SumNullable(selector), afterEffect);
        }

        private static double? SumNullable<TSource>(this IEnumerable<TSource> source, Func<TSource, double?> selector)
        {
            return source.Select(selector).SumNullable();
        }

        private static double? SumNullable(this IEnumerable<double?> source)
        {
            double? num = 0;
            foreach (double? nullable in source)
            {
                num += nullable;
            }
            return num;
        }

        #endregion

        #region COUNT

        public static ContinuousValue<int> ContinuousCount<T>(
            this ObservableCollection<T> input) where T : INotifyPropertyChanged
        {
            return new ContinuousValue<T, int, int>(input, null, (list, selector) => list.Count);
        }
        public static ContinuousValue<int> ContinuousCount<T>(
            this ObservableCollection<T> input,
            Action<int> afterEffect) where T : INotifyPropertyChanged
        {
            return new ContinuousValue<T, int, int>(input, null, (list, selector) => list.Count, afterEffect);
        }

        public static ContinuousValue<int> ContinuousCount<T>(
            this ReadOnlyObservableCollection<T> input) where T : INotifyPropertyChanged
        {
            return new ContinuousValue<T, int, int>(input, null, (list, selector) => list.Count);
        }
        public static ContinuousValue<int> ContinuousCount<T>(
            this ReadOnlyObservableCollection<T> input,
            Action<int> afterEffect) where T : INotifyPropertyChanged
        {
            return new ContinuousValue<T, int, int>(input, null, (list, selector) => list.Count, afterEffect);
        }

        public static ContinuousValue<int> ContinuousCount<T>(
            this ReadOnlyContinuousCollection<T> input) where T : INotifyPropertyChanged
        {
            return new ContinuousValue<T, int, int>(input, null, (list, selector) => list.Count);
        }
        public static ContinuousValue<int> ContinuousCount<T>(
            this ReadOnlyContinuousCollection<T> input,
            Action<int> afterEffect) where T : INotifyPropertyChanged
        {
            return new ContinuousValue<T, int, int>(input, null, (list, selector) => list.Count, afterEffect);
        }


        #endregion

        #region MIN
        #region -- Int
        public static ContinuousValue<int> ContinuousMin<T>(
            this ObservableCollection<T> input,
            Expression<Func<T, int>> minFunc) where T : INotifyPropertyChanged
        {
            return new ContinuousValue<T, int, int>(input, minFunc, (list, selector) => list.Count > 0 ? list.Min(selector) : int.MaxValue);
        }
        public static ContinuousValue<int> ContinuousMin<T>(
            this ObservableCollection<T> input,
            Expression<Func<T, int>> minFunc,
            Action<int> afterEffect) where T : INotifyPropertyChanged
        {
            return new ContinuousValue<T, int, int>(input, minFunc, (list, selector) => list.Count > 0 ? list.Min(selector) : int.MaxValue, afterEffect);
        }
        
        public static ContinuousValue<int> ContinuousMin<T>(
            this ReadOnlyObservableCollection<T> input,
            Expression<Func<T, int>> minFunc) where T : INotifyPropertyChanged
        {
            return new ContinuousValue<T, int, int>(input, minFunc, (list, selector) => list.Count > 0 ? list.Min(selector) : int.MaxValue);
        }
        public static ContinuousValue<int> ContinuousMin<T>(
            this ReadOnlyObservableCollection<T> input,
            Expression<Func<T, int>> minFunc,
            Action<int> afterEffect) where T : INotifyPropertyChanged
        {
            return new ContinuousValue<T, int, int>(input, minFunc, (list, selector) => list.Count > 0 ? list.Min(selector) : int.MaxValue, afterEffect);
        }

        public static ContinuousValue<int> ContinuousMin<T>(
            this ReadOnlyContinuousCollection<T> input,
            Expression<Func<T, int>> minFunc) where T : INotifyPropertyChanged
        {
            return new ContinuousValue<T, int, int>(input, minFunc, (list, selector) => list.Count > 0 ? list.Min(selector) : int.MaxValue);
        }
        public static ContinuousValue<int> ContinuousMin<T>(
            this ReadOnlyContinuousCollection<T> input,
            Expression<Func<T, int>> minFunc,
            Action<int> afterEffect) where T : INotifyPropertyChanged
        {
            return new ContinuousValue<T, int, int>(input, minFunc, (list, selector) => list.Count > 0 ? list.Min(selector) : int.MaxValue, afterEffect);
        }
        #endregion

        #region -- Double
        public static ContinuousValue<double> ContinuousMin<T>(
            this ObservableCollection<T> input,
            Expression<Func<T, double>> minFunc) where T : INotifyPropertyChanged
        {
            return new ContinuousValue<T, double, double>(input, minFunc, (list, selector) => list.Count > 0 ? list.Min(selector) : double.MaxValue);
        }
        public static ContinuousValue<double> ContinuousMin<T>(
            this ObservableCollection<T> input,
            Expression<Func<T, double>> minFunc,
            Action<double> afterEffect) where T : INotifyPropertyChanged
        {
            return new ContinuousValue<T, double, double>(input, minFunc, (list, selector) => list.Count > 0 ? list.Min(selector) : double.MaxValue, afterEffect);
        }

        public static ContinuousValue<double> ContinuousMin<T>(
            this ReadOnlyObservableCollection<T> input,
            Expression<Func<T, double>> minFunc) where T : INotifyPropertyChanged
        {
            return new ContinuousValue<T, double, double>(input, minFunc, (list, selector) => list.Count > 0 ? list.Min(selector) : double.MaxValue);
        }
        public static ContinuousValue<double> ContinuousMin<T>(
            this ReadOnlyObservableCollection<T> input,
            Expression<Func<T, double>> minFunc,
            Action<double> afterEffect) where T : INotifyPropertyChanged
        {
            return new ContinuousValue<T, double, double>(input, minFunc, (list, selector) => list.Count > 0 ? list.Min(selector) : double.MaxValue, afterEffect);
        }

        public static ContinuousValue<double> ContinuousMin<T>(
            this ReadOnlyContinuousCollection<T> input,
            Expression<Func<T, double>> minFunc) where T : INotifyPropertyChanged
        {
            return new ContinuousValue<T, double, double>(input, minFunc, (list, selector) => list.Count > 0 ? list.Min(selector) : double.MaxValue);
        }
        public static ContinuousValue<double> ContinuousMin<T>(
            this ReadOnlyContinuousCollection<T> input,
            Expression<Func<T, double>> minFunc,
            Action<double> afterEffect) where T : INotifyPropertyChanged
        {
            return new ContinuousValue<T, double, double>(input, minFunc, (list, selector) => list.Count > 0 ? list.Min(selector) : double.MaxValue, afterEffect);
        }
        #endregion

        #region -- Decimal
        public static ContinuousValue<decimal> ContinuousMin<T>(
            this ObservableCollection<T> input,
            Expression<Func<T, decimal>> minFunc) where T : INotifyPropertyChanged
        {
            return new ContinuousValue<T, decimal, decimal>(input, minFunc, (list, selector) => list.Count > 0 ? list.Min(selector) : decimal.MaxValue);
        }
        public static ContinuousValue<decimal> ContinuousMin<T>(
            this ObservableCollection<T> input,
            Expression<Func<T, decimal>> minFunc,
            Action<decimal> afterEffect) where T : INotifyPropertyChanged
        {
            return new ContinuousValue<T, decimal, decimal>(input, minFunc, (list, selector) => list.Count > 0 ? list.Min(selector) : decimal.MaxValue, afterEffect);
        }

        public static ContinuousValue<decimal> ContinuousMin<T>(
            this ReadOnlyObservableCollection<T> input,
            Expression<Func<T, decimal>> minFunc) where T : INotifyPropertyChanged
        {
            return new ContinuousValue<T, decimal, decimal>(input, minFunc, (list, selector) => list.Count > 0 ? list.Min(selector) : decimal.MaxValue);
        }
        public static ContinuousValue<decimal> ContinuousMin<T>(
            this ReadOnlyObservableCollection<T> input,
            Expression<Func<T, decimal>> minFunc,
            Action<decimal> afterEffect) where T : INotifyPropertyChanged
        {
            return new ContinuousValue<T, decimal, decimal>(input, minFunc, (list, selector) => list.Count > 0 ? list.Min(selector) : decimal.MaxValue, afterEffect);
        }

        public static ContinuousValue<decimal> ContinuousMin<T>(
            this ReadOnlyContinuousCollection<T> input,
            Expression<Func<T, decimal>> minFunc) where T : INotifyPropertyChanged
        {
            return new ContinuousValue<T, decimal, decimal>(input, minFunc, (list, selector) => list.Count > 0 ? list.Min(selector) : decimal.MaxValue);
        }
        public static ContinuousValue<decimal> ContinuousMin<T>(
            this ReadOnlyContinuousCollection<T> input,
            Expression<Func<T, decimal>> minFunc,
            Action<decimal> afterEffect) where T : INotifyPropertyChanged
        {
            return new ContinuousValue<T, decimal, decimal>(input, minFunc, (list, selector) => list.Count > 0 ? list.Min(selector) : decimal.MaxValue, afterEffect);
        }
        #endregion

        #region -- Float
        public static ContinuousValue<float> ContinuousMin<T>(
            this ObservableCollection<T> input,
            Expression<Func<T, float>> minSelector) where T : INotifyPropertyChanged
        {
            return new ContinuousValue<T, float, float>(input, minSelector, (list, selector) => list.Count > 0 ? list.Min(selector) : float.MaxValue);
        }
        public static ContinuousValue<float> ContinuousMin<T>(
            this ObservableCollection<T> input,
            Expression<Func<T, float>> minSelector,
            Action<float> afterEffect) where T : INotifyPropertyChanged
        {
            return new ContinuousValue<T, float, float>(input, minSelector, (list, selector) => list.Count > 0 ? list.Min(selector) : float.MaxValue, afterEffect);
        }

        public static ContinuousValue<float> ContinuousMin<T>(
            this ReadOnlyObservableCollection<T> input,
            Expression<Func<T, float>> minSelector) where T : INotifyPropertyChanged
        {
            return new ContinuousValue<T, float, float>(input, minSelector, (list, selector) => list.Count > 0 ? list.Min(selector) : float.MaxValue);
        }
        public static ContinuousValue<float> ContinuousMin<T>(
            this ReadOnlyObservableCollection<T> input,
            Expression<Func<T, float>> minSelector,
            Action<float> afterEffect) where T : INotifyPropertyChanged
        {
            return new ContinuousValue<T, float, float>(input, minSelector, (list, selector) => list.Count > 0 ? list.Min(selector) : float.MaxValue, afterEffect);
        }

        public static ContinuousValue<float> ContinuousMin<T>(
            this ReadOnlyContinuousCollection<T> input,
            Expression<Func<T, float>> minSelector) where T : INotifyPropertyChanged
        {
            return new ContinuousValue<T, float, float>(input, minSelector, (list, selector) => list.Count > 0 ? list.Min(selector) : float.MaxValue);
        }
        public static ContinuousValue<float> ContinuousMin<T>(
            this ReadOnlyContinuousCollection<T> input,
            Expression<Func<T, float>> minSelector,
            Action<float> afterEffect) where T : INotifyPropertyChanged
        {
            return new ContinuousValue<T, float, float>(input, minSelector, (list, selector) => list.Count > 0 ? list.Min(selector) : float.MaxValue, afterEffect);
        }


        #endregion

        #region -- Long
        public static ContinuousValue<long> ContinuousMin<T>(
            this ObservableCollection<T> input,
            Expression<Func<T, long>> minSelector) where T : INotifyPropertyChanged
        {
            return new ContinuousValue<T, long, long>(input, minSelector, (list, selector) => list.Count > 0 ? list.Min(selector) : long.MaxValue);
        }
        public static ContinuousValue<long> ContinuousMin<T>(
            this ObservableCollection<T> input,
            Expression<Func<T, long>> minSelector,
            Action<long> afterEffect) where T : INotifyPropertyChanged
        {
            return new ContinuousValue<T, long, long>(input, minSelector, (list, selector) => list.Count > 0 ? list.Min(selector) : long.MaxValue, afterEffect);
        }

        public static ContinuousValue<long> ContinousMin<T>(
            this ReadOnlyObservableCollection<T> input,
            Expression<Func<T, long>> minSelector) where T : INotifyPropertyChanged
        {
            return new ContinuousValue<T, long, long>(input, minSelector, (list, selector) => list.Count > 0 ? list.Min(selector) : long.MaxValue);
        }
        public static ContinuousValue<long> ContinousMin<T>(
            this ReadOnlyObservableCollection<T> input,
            Expression<Func<T, long>> minSelector,
            Action<long> afterEffect) where T : INotifyPropertyChanged
        {
            return new ContinuousValue<T, long, long>(input, minSelector, (list, selector) => list.Count > 0 ? list.Min(selector) : long.MaxValue, afterEffect);
        }

        public static ContinuousValue<long> ContinuousMin<T>(
            this ReadOnlyContinuousCollection<T> input,
            Expression<Func<T, long>> minSelector) where T : INotifyPropertyChanged
        {
            return new ContinuousValue<T, long, long>(input, minSelector, (list, selector) => list.Count > 0 ? list.Min(selector) : long.MaxValue);
        }
        public static ContinuousValue<long> ContinuousMin<T>(
            this ReadOnlyContinuousCollection<T> input,
            Expression<Func<T, long>> minSelector,
            Action<long> afterEffect) where T : INotifyPropertyChanged
        {
            return new ContinuousValue<T, long, long>(input, minSelector, (list, selector) => list.Count > 0 ? list.Min(selector) : long.MaxValue, afterEffect);
        }
        #endregion

        #region -- Nullable
        
        public static ContinuousValue<double> ContinuousMin<T>(
          this ReadOnlyContinuousCollection<T> input,
          Expression<Func<T, double?>> minSelector) where T : INotifyPropertyChanged
        {
            return new ContinuousValue<T, double?, double>(input, minSelector, (list, selector) => list.Count > 0 ? list.Min(selector).GetValueOrDefault() : double.MaxValue);
        }
        
        public static ContinuousValue<decimal> ContinuousMin<T>(
            this ReadOnlyContinuousCollection<T> input,
            Expression<Func<T, decimal?>> minSelector) where T : INotifyPropertyChanged
        {
            return new ContinuousValue<T, decimal?, decimal>(input, minSelector, (list, selector) => list.Count > 0 ? list.Min(selector).GetValueOrDefault() : decimal.MaxValue);
        }

        #endregion
        #endregion

        #region AVG
        #region -- Decimal
        public static ContinuousValue<decimal> ContinuousAverage<T>(
            this ObservableCollection<T> input,
            Expression<Func<T, decimal>> averageSelector) where T : INotifyPropertyChanged
        {
            return new ContinuousValue<T, decimal, decimal>(input, averageSelector, (list, selector) => list.Average(selector));
        }
        public static ContinuousValue<decimal> ContinuousAverage<T>(
            this ObservableCollection<T> input,
            Expression<Func<T, decimal>> averageSelector,
            Action<decimal> afterEffect) where T : INotifyPropertyChanged
        {
            return new ContinuousValue<T, decimal, decimal>(input, averageSelector, (list, selector) => list.Average(selector), afterEffect);
        }

        public static ContinuousValue<decimal> ContinuousAverage<T>(
            this ReadOnlyObservableCollection<T> input,
            Expression<Func<T, decimal>> averageSelector) where T : INotifyPropertyChanged
        {
            return new ContinuousValue<T, decimal, decimal>(input, averageSelector, (list, selector) => list.Average(selector));
        }
        public static ContinuousValue<decimal> ContinuousAverage<T>(
            this ReadOnlyObservableCollection<T> input,
            Expression<Func<T, decimal>> averageSelector,
            Action<decimal> afterEffect) where T : INotifyPropertyChanged
        {
            return new ContinuousValue<T, decimal, decimal>(input, averageSelector, (list, selector) => list.Average(selector), afterEffect);
        }

        public static ContinuousValue<decimal> ContinuousAverage<T>(
            this ReadOnlyContinuousCollection<T> input,
            Expression<Func<T, decimal>> averageSelector) where T : INotifyPropertyChanged
        {
            return new ContinuousValue<T, decimal, decimal>(input, averageSelector, (list, selector) => list.Average(selector));
        }
        public static ContinuousValue<decimal> ContinuousAverage<T>(
            this ReadOnlyContinuousCollection<T> input,
            Expression<Func<T, decimal>> averageSelector,
            Action<decimal> afterEffect) where T : INotifyPropertyChanged
        {
            return new ContinuousValue<T, decimal, decimal>(input, averageSelector, (list, selector) => list.Average(selector), afterEffect);
        }
        #endregion

        #region -- Float
        public static ContinuousValue<float> ContinuousAverage<T>(
            this ObservableCollection<T> input,
            Expression<Func<T, float>> averageSelector) where T : INotifyPropertyChanged
        {
            return new ContinuousValue<T,float,  float>(input, averageSelector, (list, selector) => list.Average(selector));
        }
        public static ContinuousValue<float> ContinuousAverage<T>(
            this ObservableCollection<T> input,
            Expression<Func<T, float>> averageSelector,
            Action<float> afterEffect) where T : INotifyPropertyChanged
        {
            return new ContinuousValue<T, float, float>(input, averageSelector, (list, selector) => list.Average(selector), afterEffect);
        }

        public static ContinuousValue<float> ContinuousAverage<T>(
            this ReadOnlyObservableCollection<T> input,
            Expression<Func<T, float>> averageSelector) where T : INotifyPropertyChanged
        {
            return new ContinuousValue<T, float, float>(input, averageSelector, (list, selector) => list.Average(selector));
        }
        public static ContinuousValue<float> ContinuousAverage<T>(
            this ReadOnlyObservableCollection<T> input,
            Expression<Func<T, float>> averageSelector,
            Action<float> afterEffect) where T : INotifyPropertyChanged
        {
            return new ContinuousValue<T, float, float>(input, averageSelector, (list, selector) => list.Average(selector), afterEffect);
        }

        public static ContinuousValue<float> ContinuousAverage<T>(
            this ReadOnlyContinuousCollection<T> input,
            Expression<Func<T, float>> averageSelector) where T : INotifyPropertyChanged
        {
            return new ContinuousValue<T, float, float>(input, averageSelector, (list, selector) => list.Average(selector));
        }
        public static ContinuousValue<float> ContinuousAverage<T>(
            this ReadOnlyContinuousCollection<T> input,
            Expression<Func<T, float>> averageSelector,
            Action<float> afterEffect) where T : INotifyPropertyChanged
        {
            return new ContinuousValue<T, float, float>(input, averageSelector, (list, selector) => list.Average(selector), afterEffect);
        }
        #endregion

        #region -- Double
        public static ContinuousValue<double> ContinuousAverage<T>(
           this ObservableCollection<T> input,
           Expression<Func<T, double>> averageSelector) where T : INotifyPropertyChanged
        {
            return new ContinuousValue<T, double, double>(input, averageSelector, (list, selector) => list.Average(selector));
        }
        public static ContinuousValue<double> ContinuousAverage<T>(
           this ObservableCollection<T> input,
           Expression<Func<T, double>> averageSelector,
            Action<double> afterEffect) where T : INotifyPropertyChanged
        {
            return new ContinuousValue<T, double, double>(input, averageSelector, (list, selector) => list.Average(selector), afterEffect);
        }

        public static ContinuousValue<double> ContinuousAverage<T>(
            this ReadOnlyObservableCollection<T> input,
            Expression<Func<T, double>> averageSelector) where T : INotifyPropertyChanged
        {
            return new ContinuousValue<T, double, double>(input, averageSelector, (list, selector) => list.Average(selector));
        }
        public static ContinuousValue<double> ContinuousAverage<T>(
            this ReadOnlyObservableCollection<T> input,
            Expression<Func<T, double>> averageSelector,
            Action<double> afterEffect) where T : INotifyPropertyChanged
        {
            return new ContinuousValue<T, double, double>(input, averageSelector, (list, selector) => list.Average(selector), afterEffect);
        }

        public static ContinuousValue<double> ContinuousAverage<T>(
            this ReadOnlyContinuousCollection<T> input,
            Expression<Func<T, double>> averageSelector) where T : INotifyPropertyChanged
        {
            return new ContinuousValue<T, double, double>(input, averageSelector, (list, selector) => list.Average(selector));
        }
        public static ContinuousValue<double> ContinuousAverage<T>(
            this ReadOnlyContinuousCollection<T> input,
            Expression<Func<T, double>> averageSelector,
            Action<double> afterEffect) where T : INotifyPropertyChanged
        {
            return new ContinuousValue<T, double, double>(input, averageSelector, (list, selector) => list.Average(selector), afterEffect);
        }
        #endregion

        
        #endregion

        #region MAX
       
        #region -- Int
        public static ContinuousValue<int> ContinuousMax<T>(
           this ObservableCollection<T> input,
           Expression<Func<T, int>> maxSelector) where T : INotifyPropertyChanged
        {
            return new ContinuousValue<T, int, int>(input, maxSelector, (list, selector) => list.Count > 0 ? list.Max(selector) : int.MinValue);
        }
        public static ContinuousValue<int> ContinuousMax<T>(
           this ObservableCollection<T> input,
           Expression<Func<T, int>> maxSelector,
            Action<int> afterEffect) where T : INotifyPropertyChanged
        {
            return new ContinuousValue<T, int, int>(input, maxSelector, (list, selector) => list.Count > 0 ? list.Max(selector) : int.MinValue, afterEffect);
        }

        public static ContinuousValue<int> ContinousMax<T>(
            this ReadOnlyObservableCollection<T> input,
            Expression<Func<T, int>> maxSelector) where T : INotifyPropertyChanged
        {
            return new ContinuousValue<T, int, int>(input, maxSelector, (list, selector) => list.Count > 0 ? list.Max(selector) : int.MinValue);
        }
        public static ContinuousValue<int> ContinousMax<T>(
            this ReadOnlyObservableCollection<T> input,
            Expression<Func<T, int>> maxSelector,
            Action<int> afterEffect) where T : INotifyPropertyChanged
        {
            return new ContinuousValue<T, int, int>(input, maxSelector, (list, selector) => list.Count > 0 ? list.Max(selector) : int.MinValue, afterEffect);
        }


        public static ContinuousValue<int> ContinuousMax<T>(
            this ReadOnlyContinuousCollection<T> input,
            Expression<Func<T, int>> maxSelector) where T : INotifyPropertyChanged
        {
            return new ContinuousValue<T, int, int>(input, maxSelector, (list, selector) => list.Count > 0 ? list.Max(selector) : int.MinValue);
        }
        public static ContinuousValue<int> ContinuousMax<T>(
            this ReadOnlyContinuousCollection<T> input,
            Expression<Func<T, int>> maxSelector,
            Action<int> afterEffect) where T : INotifyPropertyChanged
        {
            return new ContinuousValue<T, int, int>(input, maxSelector, (list, selector) => list.Count > 0 ? list.Max(selector) : int.MinValue, afterEffect);
        }
        #endregion

        #region -- Long
        public static ContinuousValue<long> ContinuousMax<T>(
           this ObservableCollection<T> input,
           Expression<Func<T, long>> maxSelector) where T : INotifyPropertyChanged
        {
            return new ContinuousValue<T, long, long>(input, maxSelector, (list, selector) => list.Count > 0 ? list.Max(selector) : long.MinValue);
        }
        public static ContinuousValue<long> ContinuousMax<T>(
           this ObservableCollection<T> input,
           Expression<Func<T, long>> maxSelector,
            Action<long> afterEffect) where T : INotifyPropertyChanged
        {
            return new ContinuousValue<T, long, long>(input, maxSelector, (list, selector) => list.Count > 0 ? list.Max(selector) : long.MinValue, afterEffect);
        }

        public static ContinuousValue<long> ContinousMax<T>(
            this ReadOnlyObservableCollection<T> input,
            Expression<Func<T, long>> maxSelector) where T : INotifyPropertyChanged
        {
            return new ContinuousValue<T, long, long>(input, maxSelector, (list, selector) => list.Count > 0 ? list.Max(selector) : long.MinValue);
        }
        public static ContinuousValue<long> ContinousMax<T>(
            this ReadOnlyObservableCollection<T> input,
            Expression<Func<T, long>> maxSelector,
            Action<long> afterEffect) where T : INotifyPropertyChanged
        {
            return new ContinuousValue<T, long, long>(input, maxSelector, (list, selector) => list.Count > 0 ? list.Max(selector) : long.MinValue, afterEffect);
        }

        public static ContinuousValue<long> ContinuousMax<T>(
            this ReadOnlyContinuousCollection<T> input,
            Expression<Func<T, long>> maxSelector) where T : INotifyPropertyChanged
        {
            return new ContinuousValue<T, long, long>(input, maxSelector, (list, selector) => list.Count > 0 ? list.Max(selector) : long.MinValue);
        }
        public static ContinuousValue<long> ContinuousMax<T>(
            this ReadOnlyContinuousCollection<T> input,
            Expression<Func<T, long>> maxSelector,
            Action<long> afterEffect) where T : INotifyPropertyChanged
        {
            return new ContinuousValue<T, long, long>(input, maxSelector, (list, selector) => list.Count > 0 ? list.Max(selector) : long.MinValue, afterEffect);
        }
        #endregion

        #region -- Decimal
        public static ContinuousValue<decimal> ContinuousMax<T>(
           this ObservableCollection<T> input,
           Expression<Func<T, decimal>> maxSelector) where T : INotifyPropertyChanged
        {
            return new ContinuousValue<T, decimal, decimal>(input, maxSelector, (list, selector) => list.Count > 0 ? list.Max(selector) : decimal.MinValue);
        }
        public static ContinuousValue<decimal> ContinuousMax<T>(
           this ObservableCollection<T> input,
           Expression<Func<T, decimal>> maxSelector,
            Action<decimal> afterEffect) where T : INotifyPropertyChanged
        {
            return new ContinuousValue<T, decimal, decimal>(input, maxSelector, (list, selector) => list.Count > 0 ? list.Max(selector) : decimal.MinValue, afterEffect);
        }

        public static ContinuousValue<decimal> ContinousMax<T>(
            this ReadOnlyObservableCollection<T> input,
            Expression<Func<T, decimal>> maxSelector) where T : INotifyPropertyChanged
        {
            return new ContinuousValue<T, decimal, decimal>(input, maxSelector, (list, selector) => list.Count > 0 ? list.Max(selector) : decimal.MinValue);
        }
        public static ContinuousValue<decimal> ContinousMax<T>(
            this ReadOnlyObservableCollection<T> input,
            Expression<Func<T, decimal>> maxSelector,
            Action<decimal> afterEffect) where T : INotifyPropertyChanged
        {
            return new ContinuousValue<T, decimal, decimal>(input, maxSelector, (list, selector) => list.Count > 0 ? list.Max(selector) : decimal.MinValue, afterEffect);
        }


        public static ContinuousValue<decimal> ContinuousMax<T>(
            this ReadOnlyContinuousCollection<T> input,
            Expression<Func<T, decimal>> maxSelector) where T : INotifyPropertyChanged
        {
            return new ContinuousValue<T, decimal, decimal>(input, maxSelector, (list, selector) => list.Count > 0 ? list.Max(selector) : decimal.MinValue);
        }
        public static ContinuousValue<decimal> ContinuousMax<T>(
            this ReadOnlyContinuousCollection<T> input,
            Expression<Func<T, decimal>> maxSelector,
            Action<decimal> afterEffect) where T : INotifyPropertyChanged
        {
            return new ContinuousValue<T, decimal, decimal>(input, maxSelector, (list, selector) => list.Count > 0 ? list.Max(selector) : decimal.MinValue, afterEffect);
        }
        #endregion

        #region -- Double
        public static ContinuousValue<double> ContinuousMax<T>(
           this ObservableCollection<T> input,
           Expression<Func<T, double>> maxSelector) where T : INotifyPropertyChanged
        {
            return new ContinuousValue<T, double, double>(input, maxSelector, (list, selector) => list.Count > 0 ? list.Max(selector) : double.MinValue);
        }
        public static ContinuousValue<double> ContinuousMax<T>(
           this ObservableCollection<T> input,
           Expression<Func<T, double>> maxSelector,
           Action<double> afterEffect) where T : INotifyPropertyChanged
        {
            return new ContinuousValue<T, double, double>(input, maxSelector, (list, selector) => list.Count > 0 ? list.Max(selector) : double.MinValue, afterEffect);
        }


        public static ContinuousValue<double> ContinousMax<T>(
            this ReadOnlyObservableCollection<T> input,
            Expression<Func<T, double>> maxSelector) where T : INotifyPropertyChanged
        {
            return new ContinuousValue<T, double, double>(input, maxSelector, (list, selector) => list.Count > 0 ? list.Max(selector) : double.MinValue);
        }
        public static ContinuousValue<double> ContinousMax<T>(
            this ReadOnlyObservableCollection<T> input,
            Expression<Func<T, double>> maxSelector,
            Action<double> afterEffect) where T : INotifyPropertyChanged
        {
            return new ContinuousValue<T, double, double>(input, maxSelector, (list, selector) => list.Count > 0 ? list.Max(selector) : double.MinValue, afterEffect);
        }

        public static ContinuousValue<double> ContinuousMax<T>(
            this ReadOnlyContinuousCollection<T> input,
            Expression<Func<T, double>> maxSelector) where T : INotifyPropertyChanged
        {
            return new ContinuousValue<T, double, double>(input, maxSelector, (list, selector) => list.Count > 0 ? list.Max(selector) : double.MinValue);
        }
        public static ContinuousValue<double> ContinuousMax<T>(
            this ReadOnlyContinuousCollection<T> input,
            Expression<Func<T, double>> maxSelector,
            Action<double> afterEffect) where T : INotifyPropertyChanged
        {
            return new ContinuousValue<T, double, double>(input, maxSelector, (list, selector) => list.Count > 0 ? list.Max(selector) : double.MinValue, afterEffect);
        }
        #endregion

        #region -- Float
        public static ContinuousValue<float> ContinuousMax<T>(
           this ObservableCollection<T> input,
           Expression<Func<T, float>> maxSelector) where T : INotifyPropertyChanged
        {
            return new ContinuousValue<T, float, float>(input, maxSelector, (list, selector) => list.Count > 0 ? list.Max(selector) : float.MinValue);
        }
        public static ContinuousValue<float> ContinuousMax<T>(
           this ObservableCollection<T> input,
           Expression<Func<T, float>> maxSelector,
            Action<float> afterEffect) where T : INotifyPropertyChanged
        {
            return new ContinuousValue<T, float, float>(input, maxSelector, (list, selector) => list.Count > 0 ? list.Max(selector) : float.MinValue, afterEffect);
        }

        public static ContinuousValue<float> ContinousMax<T>(
            this ReadOnlyObservableCollection<T> input,
            Expression<Func<T, float>> maxSelector) where T : INotifyPropertyChanged
        {
            return new ContinuousValue<T, float, float>(input, maxSelector, (list, selector) => list.Count > 0 ? list.Max(selector) : float.MinValue);
        }
        public static ContinuousValue<float> ContinousMax<T>(
            this ReadOnlyObservableCollection<T> input,
            Expression<Func<T, float>> maxSelector,
            Action<float> afterEffect) where T : INotifyPropertyChanged
        {
            return new ContinuousValue<T, float, float>(input, maxSelector, (list, selector) => list.Count > 0 ? list.Max(selector) : float.MinValue, afterEffect);
        }

        public static ContinuousValue<float> ContinuousMax<T>(
            this ReadOnlyContinuousCollection<T> input,
            Expression<Func<T, float>> maxSelector) where T : INotifyPropertyChanged
        {
            return new ContinuousValue<T, float, float>(input, maxSelector, (list, selector) => list.Count > 0 ? list.Max(selector) : float.MinValue);
        }
        public static ContinuousValue<float> ContinuousMax<T>(
            this ReadOnlyContinuousCollection<T> input,
            Expression<Func<T, float>> maxSelector,
            Action<float> afterEffect) where T : INotifyPropertyChanged
        {
            return new ContinuousValue<T, float, float>(input, maxSelector, (list, selector) => list.Count > 0 ? list.Max(selector) : float.MinValue, afterEffect);
        }
        #endregion

        #region --Nullables

        public static ContinuousValue<double> ContinuousMax<T>(
            this ReadOnlyContinuousCollection<T> input,
            Expression<Func<T, double?>> maxSelector) where T : INotifyPropertyChanged
        {
            return new ContinuousValue<T, double?, double>(input, maxSelector, (list, selector) => list.Count > 0 ? list.Max(selector).GetValueOrDefault() : double.MinValue);
        }
        public static ContinuousValue<double> ContinuousMax<T>(
            this ReadOnlyContinuousCollection<T> input,
            Expression<Func<T, double?>> maxSelector,
            Action<double> afterEffect) where T : INotifyPropertyChanged
        {
            return new ContinuousValue<T, double?, double>(input, maxSelector, (list, selector) => list.Count > 0 ? list.Max(selector).GetValueOrDefault() : double.MinValue, afterEffect);
        }

        public static ContinuousValue<decimal> ContinuousMax<T>(
            this ReadOnlyContinuousCollection<T> input,
            Expression<Func<T, decimal?>> maxSelector) where T : INotifyPropertyChanged
        {
            return new ContinuousValue<T, decimal?, decimal>(input, maxSelector, (list, selector) => list.Count > 0 ? list.Max(selector).GetValueOrDefault() : decimal.MinValue);
        }

        #endregion
        #endregion

        #region STDDEV
        #region -- Int
        public static ContinuousValue<double> ContinuousStdDev<T>(
            this ObservableCollection<T> input,
            Expression<Func<T, int>> columnSelector) where T : INotifyPropertyChanged
        {
            return new ContinuousValue<T, int, double>(input, columnSelector, (list, selector) => StdDev.Compute(selector, list));
        }
        public static ContinuousValue<double> ContinuousStdDev<T>(
            this ObservableCollection<T> input,
            Expression<Func<T, int>> columnSelector,
            Action<double> afterEffect) where T : INotifyPropertyChanged
        {
            return new ContinuousValue<T, int, double>(input, columnSelector, (list, selector) => StdDev.Compute(selector, list), afterEffect);
        }


        public static ContinuousValue<double> ContinuousStdDev<T>(
            this ReadOnlyObservableCollection<T> input,
            Expression<Func<T, int>> columnSelector) where T : INotifyPropertyChanged
        {
            return new ContinuousValue<T, int, double>(input, columnSelector, (list, selector) => StdDev.Compute(selector, list));
        }
        public static ContinuousValue<double> ContinuousStdDev<T>(
            this ReadOnlyObservableCollection<T> input,
            Expression<Func<T, int>> columnSelector,
            Action<double> afterEffect) where T : INotifyPropertyChanged
        {
            return new ContinuousValue<T, int, double>(input, columnSelector, (list, selector) => StdDev.Compute(selector, list), afterEffect);
        }

        public static ContinuousValue<double> ContinuousStdDev<T>(
            this ReadOnlyContinuousCollection<T> input,
            Expression<Func<T, int>> columnSelector) where T : INotifyPropertyChanged
        {
            return new ContinuousValue<T, int, double>(input, columnSelector, (list, selector) => StdDev.Compute(selector, list));
        }
        public static ContinuousValue<double> ContinuousStdDev<T>(
            this ReadOnlyContinuousCollection<T> input,
            Expression<Func<T, int>> columnSelector,
            Action<double> afterEffect) where T : INotifyPropertyChanged
        {
            return new ContinuousValue<T, int, double>(input, columnSelector, (list, selector) => StdDev.Compute(selector, list), afterEffect);
        }
        #endregion

        #region -- Long
        public static ContinuousValue<double> ContinuousStdDev<T>(
            this ObservableCollection<T> input,
            Expression<Func<T, long>> columnSelector) where T : INotifyPropertyChanged
        {
            return new ContinuousValue<T, long, double>(input, columnSelector, (list, selector) => StdDev.Compute(selector, list));
        }
        public static ContinuousValue<double> ContinuousStdDev<T>(
            this ObservableCollection<T> input,
            Expression<Func<T, long>> columnSelector,
            Action<double> afterEffect) where T : INotifyPropertyChanged
        {
            return new ContinuousValue<T, long, double>(input, columnSelector, (list, selector) => StdDev.Compute(selector, list), afterEffect);
        }


        public static ContinuousValue<double> ContinuousStdDev<T>(
            this ReadOnlyObservableCollection<T> input,
            Expression<Func<T, long>> columnSelector) where T : INotifyPropertyChanged
        {
            return new ContinuousValue<T, long, double>(input, columnSelector, (list, selector) => StdDev.Compute(selector, list));
        }
        public static ContinuousValue<double> ContinuousStdDev<T>(
            this ReadOnlyObservableCollection<T> input,
            Expression<Func<T, long>> columnSelector,
            Action<double> afterEffect) where T : INotifyPropertyChanged
        {
            return new ContinuousValue<T, long, double>(input, columnSelector, (list, selector) => StdDev.Compute(selector, list), afterEffect);
        }

        public static ContinuousValue<double> ContinuousStdDev<T>(
            this ReadOnlyContinuousCollection<T> input,
            Expression<Func<T, long>> columnSelector) where T : INotifyPropertyChanged
        {
            return new ContinuousValue<T, long, double>(input, columnSelector, (list, selector) => StdDev.Compute(selector, list));
        }
        public static ContinuousValue<double> ContinuousStdDev<T>(
            this ReadOnlyContinuousCollection<T> input,
            Expression<Func<T, long>> columnSelector,
            Action<double> afterEffect) where T : INotifyPropertyChanged
        {
            return new ContinuousValue<T, long, double>(input, columnSelector, (list, selector) => StdDev.Compute(selector, list), afterEffect);
        }
        #endregion

        #region -- Decimal
        public static ContinuousValue<double> ContinuousStdDev<T>(
            this ObservableCollection<T> input,
            Expression<Func<T, decimal>> columnSelector) where T : INotifyPropertyChanged
        {
            return new ContinuousValue<T, decimal, double>(input, columnSelector, (list, selector) => StdDev.Compute(selector, list));
        }
        public static ContinuousValue<double> ContinuousStdDev<T>(
            this ObservableCollection<T> input,
            Expression<Func<T, decimal>> columnSelector,
            Action<double> afterEffect) where T : INotifyPropertyChanged
        {
            return new ContinuousValue<T, decimal, double>(input, columnSelector, (list, selector) => StdDev.Compute(selector, list), afterEffect);
        }

        public static ContinuousValue<double> ContinuousStdDev<T>(
            this ReadOnlyObservableCollection<T> input,
            Expression<Func<T, decimal>> columnSelector) where T : INotifyPropertyChanged
        {
            return new ContinuousValue<T, decimal, double>(input, columnSelector, (list, selector) => StdDev.Compute(selector, list));
        }
        public static ContinuousValue<double> ContinuousStdDev<T>(
            this ReadOnlyObservableCollection<T> input,
            Expression<Func<T, decimal>> columnSelector,
            Action<double> afterEffect) where T : INotifyPropertyChanged
        {
            return new ContinuousValue<T, decimal, double>(input, columnSelector, (list, selector) => StdDev.Compute(selector, list), afterEffect);
        }


        public static ContinuousValue<double> ContinuousStdDev<T>(
            this ReadOnlyContinuousCollection<T> input,
            Expression<Func<T, decimal>> columnSelector) where T : INotifyPropertyChanged
        {
            return new ContinuousValue<T, decimal, double>(input, columnSelector, (list, selector) => StdDev.Compute(selector, list));
        }
        public static ContinuousValue<double> ContinuousStdDev<T>(
            this ReadOnlyContinuousCollection<T> input,
            Expression<Func<T, decimal>> columnSelector,
            Action<double> afterEffect) where T : INotifyPropertyChanged
        {
            return new ContinuousValue<T, decimal, double>(input, columnSelector, (list, selector) => StdDev.Compute(selector, list), afterEffect);
        }

        #endregion

        #region -- Float
        public static ContinuousValue<double> ContinuousStdDev<T>(
            this ObservableCollection<T> input,
            Expression<Func<T, float>> columnSelector) where T : INotifyPropertyChanged
        {
            return new ContinuousValue<T, float, double>(input, columnSelector, (list, selector) => StdDev.Compute(selector, list));
        }
        public static ContinuousValue<double> ContinuousStdDev<T>(
            this ObservableCollection<T> input,
            Expression<Func<T, float>> columnSelector,
            Action<double> afterEffect) where T : INotifyPropertyChanged
        {
            return new ContinuousValue<T, float, double>(input, columnSelector, (list, selector) => StdDev.Compute(selector, list), afterEffect);
        }

        public static ContinuousValue<double> ContinuousStdDev<T>(
            this ReadOnlyObservableCollection<T> input,
            Expression<Func<T, float>> columnSelector) where T : INotifyPropertyChanged
        {
            return new ContinuousValue<T, float, double>(input, columnSelector, (list, selector) => StdDev.Compute(selector, list));
        }
        public static ContinuousValue<double> ContinuousStdDev<T>(
            this ReadOnlyObservableCollection<T> input,
            Expression<Func<T, float>> columnSelector,
            Action<double> afterEffect) where T : INotifyPropertyChanged
        {
            return new ContinuousValue<T, float, double>(input, columnSelector, (list, selector) => StdDev.Compute(selector, list), afterEffect);
        }

        public static ContinuousValue<double> ContinuousStdDev<T>(
            this ReadOnlyContinuousCollection<T> input,
            Expression<Func<T, float>> columnSelector) where T : INotifyPropertyChanged
        {
            return new ContinuousValue<T, float, double>(input, columnSelector, (list, selector) => StdDev.Compute(selector, list));
        }
        public static ContinuousValue<double> ContinuousStdDev<T>(
            this ReadOnlyContinuousCollection<T> input,
            Expression<Func<T, float>> columnSelector,
            Action<double> afterEffect) where T : INotifyPropertyChanged
        {
            return new ContinuousValue<T, float, double>(input, columnSelector, (list, selector) => StdDev.Compute(selector, list), afterEffect);
        }
        #endregion

        #region -- Double
        public static ContinuousValue<double> ContinuousStdDev<T>(
            this ObservableCollection<T> input,
            Expression<Func<T, double>> columnSelector) where T : INotifyPropertyChanged
        {
            return new ContinuousValue<T, double, double>(input, columnSelector, (list, selector) => StdDev.Compute(selector, list));
        }
        public static ContinuousValue<double> ContinuousStdDev<T>(
            this ObservableCollection<T> input,
            Expression<Func<T, double>> columnSelector,
            Action<double> afterEffect) where T : INotifyPropertyChanged
        {
            return new ContinuousValue<T, double, double>(input, columnSelector, (list, selector) => StdDev.Compute(selector, list), afterEffect);
        }

        public static ContinuousValue<double> ContinuousStdDev<T>(
            this ReadOnlyObservableCollection<T> input,
            Expression<Func<T, double>> columnSelector) where T : INotifyPropertyChanged
        {
            return new ContinuousValue<T, double, double>(input, columnSelector, (list, selector) => StdDev.Compute(selector, list));
        }
        public static ContinuousValue<double> ContinuousStdDev<T>(
            this ReadOnlyObservableCollection<T> input,
            Expression<Func<T, double>> columnSelector,
            Action<double> afterEffect) where T : INotifyPropertyChanged
        {
            return new ContinuousValue<T, double, double>(input, columnSelector, (list, selector) => StdDev.Compute(selector, list), afterEffect);
        }

        public static ContinuousValue<double> ContinuousStdDev<T>(
            this ReadOnlyContinuousCollection<T> input,
            Expression<Func<T, double>> columnSelector) where T : INotifyPropertyChanged
        {
            return new ContinuousValue<T, double, double>(input, columnSelector, (list, selector) => StdDev.Compute(selector, list));
        }
        public static ContinuousValue<double> ContinuousStdDev<T>(
            this ReadOnlyContinuousCollection<T> input,
            Expression<Func<T, double>> columnSelector,
            Action<double> afterEffect) where T : INotifyPropertyChanged
        {
            return new ContinuousValue<T, double, double>(input, columnSelector, (list, selector) => StdDev.Compute(selector, list), afterEffect);
        }

        #endregion
        #endregion        

        #region CONTAINS
        public static ContinuousValue<bool> ContinuousContains<T>(
            this ObservableCollection<T> input,
            T item) where T: INotifyPropertyChanged
        {
            return new ContinuousValue<T, bool, bool>(input, null, (list, selector) => list.Contains(item));
        }
        public static ContinuousValue<bool> ContinuousContains<T>(
            this ObservableCollection<T> input,
            T item,
            Action<bool> afterEffect) where T : INotifyPropertyChanged
        {
            return new ContinuousValue<T, bool, bool>(input, null, (list, selector) => list.Contains(item), afterEffect);
        }

        public static ContinuousValue<bool> ContinuousContains<T>(
            this ReadOnlyObservableCollection<T> input,
            T item) where T : INotifyPropertyChanged
        {
            return new ContinuousValue<T, bool, bool>(input, null, (list, selector) => list.Contains(item));
        }
        public static ContinuousValue<bool> ContinuousContains<T>(
            this ReadOnlyObservableCollection<T> input,
            T item,
            Action<bool> afterEffect) where T : INotifyPropertyChanged
        {
            return new ContinuousValue<T, bool, bool>(input, null, (list, selector) => list.Contains(item), afterEffect);
        }

        public static ContinuousValue<bool> ContinuousContains<T>(
            this ReadOnlyContinuousCollection<T> input,
            T item) where T : INotifyPropertyChanged
        {
            return new ContinuousValue<T, bool, bool>(input, null, (list, selector) => list.Contains(item));
        }
        public static ContinuousValue<bool> ContinuousContains<T>(
            this ReadOnlyContinuousCollection<T> input,
            T item,
            Action<bool> afterEffect) where T : INotifyPropertyChanged
        {
            return new ContinuousValue<T, bool, bool>(input, null, (list, selector) => list.Contains(item), afterEffect);
        }
        
        #endregion

        #region FirstOrDefault
        public static ContinuousValue<T> ContinuousFirstOrDefault<T>(
            this ObservableCollection<T> input) where T : INotifyPropertyChanged
        {
            return new ContinuousValue<T, T, T>(input, null, (list, selector) => list.FirstOrDefault());  
        }
        public static ContinuousValue<T> ContinuousFirstOrDefault<T>(
            this ObservableCollection<T> input,
            Expression<Func<T, bool>> predicate) where T : INotifyPropertyChanged
        {
            return new ContinuousValue<T, bool, T>(input, predicate, (list, selector) => list.FirstOrDefault(predicate.CachedCompile()));
        }
        public static ContinuousValue<T> ContinuousFirstOrDefault<T>(
            this ObservableCollection<T> input,
            Action<T> afterEffect) where T : INotifyPropertyChanged
        {
            return new ContinuousValue<T, T, T>(input, null, (list, selector) => list.FirstOrDefault(), afterEffect);
        }
        public static ContinuousValue<T> ContinuousFirstOrDefault<T>(
            this ObservableCollection<T> input,
            Expression<Func<T, bool>> predicate,
            Action<T> afterEffect) where T : INotifyPropertyChanged
        {
            return new ContinuousValue<T, bool, T>(input, predicate, (list, selector) => list.FirstOrDefault(predicate.CachedCompile()), afterEffect);
        }


        public static ContinuousValue<T> ContinuousFirstOrDefault<T>(
            this ReadOnlyObservableCollection<T> input) where T : INotifyPropertyChanged
        {
            return new ContinuousValue<T, T, T>(input, null, (list, selector) => list.FirstOrDefault());
        }
        public static ContinuousValue<T> ContinuousFirstOrDefault<T>(
            this ReadOnlyObservableCollection<T> input,
            Expression<Func<T, bool>> predicate) where T : INotifyPropertyChanged
        {
            return new ContinuousValue<T, bool, T>(input, predicate, (list, selector) => list.FirstOrDefault(predicate.CachedCompile()));
        }
        public static ContinuousValue<T> ContinuousFirstOrDefault<T>(
            this ReadOnlyObservableCollection<T> input,
            Action<T> afterEffect) where T : INotifyPropertyChanged
        {
            return new ContinuousValue<T, T, T>(input, null, (list, selector) => list.FirstOrDefault(), afterEffect);
        }
        public static ContinuousValue<T> ContinuousFirstOrDefault<T>(
            this ReadOnlyObservableCollection<T> input,
            Expression<Func<T, bool>> predicate,
            Action<T> afterEffect) where T : INotifyPropertyChanged
        {
            return new ContinuousValue<T, bool, T>(input, predicate, (list, selector) => list.FirstOrDefault(predicate.CachedCompile()), afterEffect);
        }

        public static ContinuousValue<T> ContinuousFirstOrDefault<T>(
            this ReadOnlyContinuousCollection<T> input) where T : INotifyPropertyChanged
        {
            return new ContinuousValue<T, T, T>(input, null, (list, selector) => list.FirstOrDefault());
        }
        public static ContinuousValue<T> ContinuousFirstOrDefault<T>(
            this ReadOnlyContinuousCollection<T> input,
            Expression<Func<T, bool>> predicate) where T : INotifyPropertyChanged
        {
            return new ContinuousValue<T, bool, T>(input, predicate, (list, selector) => list.FirstOrDefault(predicate.CachedCompile()));
        }
        public static ContinuousValue<T> ContinuousFirstOrDefault<T>(
            this ReadOnlyContinuousCollection<T> input,
            Action<T> afterEffect) where T : INotifyPropertyChanged
        {
            return new ContinuousValue<T, T, T>(input, null, (list, selector) => list.FirstOrDefault(), afterEffect);
        }
        public static ContinuousValue<T> ContinuousFirstOrDefault<T>(
            this ReadOnlyContinuousCollection<T> input,
            Expression<Func<T, bool>> predicate,
            Action<T> afterEffect) where T : INotifyPropertyChanged
        {
            return new ContinuousValue<T, bool, T>(input, predicate, (list, selector) => list.FirstOrDefault(predicate.CachedCompile()), afterEffect);
        }
        #endregion
    }
}
