using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using ContinuousLinq;
using ContinuousLinq.Aggregates;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace ContinuousLinq.OrderBookDemo.Models
{
    public static class Vwap
    {
        public static ContinuousValue<double> ContinuousVwap<T>(
            this ReadOnlyContinuousCollection<T> input,
            Func<T, double> priceSelector,
            Func<T, int> quantitySelector,
            Action<double> afterEffect)
            where T:INotifyPropertyChanged
        {
            return new ContinuousValue<T, double, double>(input, null,
                (list, selector) => ComputeVwap<T>(list, priceSelector, quantitySelector), afterEffect);
        }

        public static double ComputeVwap<T>(IList<T> list, Func<T, double> priceSelector,
            Func<T, int> quantitySelector)
        {
            int count = list.Count;
            double weightedPrice = 0.0;
            int totalQuantity = 0;

            for (int x = 0; x < count; x++)
            {
                weightedPrice += priceSelector(list[x]) *
                    quantitySelector(list[x]);
                totalQuantity += quantitySelector(list[x]);
            }

            double vwap = weightedPrice / totalQuantity;
            return vwap;
        }

        
    }
}
