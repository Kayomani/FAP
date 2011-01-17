using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ContinuousLinq.Aggregates
{
    internal static class StdDev
    {
        public static double Compute<T>(Func<T, int> columnSelector, ICollection<T> dataList)
        {
            double finalValue = 0;
            double variance = 0.0;

            int count = dataList.Count;
            double average = dataList.Average(columnSelector);

            if (count == 0) return finalValue;

            foreach (T item in dataList)
            {
                int columnValue = columnSelector(item);
                variance += Math.Pow(columnValue - average, 2);
            }

            finalValue = Math.Sqrt(variance / count);
            return finalValue;
        }

        public static double Compute<T>(Func<T, double> columnSelector, ICollection<T> dataList)
        {
            double finalValue = 0;
            double variance = 0.0;

            int count = dataList.Count;
            double average = dataList.Average(columnSelector);

            if (count == 0) return finalValue;

            foreach (T item in dataList)
            {
                double columnValue = columnSelector(item);
                variance += Math.Pow(columnValue - average, 2);
            }

            finalValue = Math.Sqrt(variance / count);
            return finalValue;
        }

        public static double Compute<T>(Func<T, float> columnSelector, ICollection<T> dataList)
        {
            double finalValue = 0;
            double variance = 0.0;

            int count = dataList.Count;
            double average = dataList.Average(columnSelector);

            if (count == 0) return finalValue;

            foreach (T item in dataList)
            {
                float columnValue = columnSelector(item);
                variance += Math.Pow(columnValue - average, 2);
            }

            finalValue = Math.Sqrt(variance / count);
            return finalValue;
        }

        public static double Compute<T>(Func<T, long> columnSelector, ICollection<T> dataList)
        {
            double finalValue = 0;
            double variance = 0.0;

            int count = dataList.Count;
            double average = dataList.Average(columnSelector);

            if (count == 0) return finalValue;

            foreach (T item in dataList)
            {
                long columnValue = columnSelector(item);
                variance += Math.Pow(columnValue - average, 2);
            }

            finalValue = Math.Sqrt(variance / count);
            return finalValue;
        }

        public static double Compute<T>(Func<T, decimal> columnSelector, ICollection<T> dataList)
        {
            double finalValue = 0;
            double variance = 0.0;

            int count = dataList.Count;
            double average = (double)dataList.Average(columnSelector);

            if (count == 0) return finalValue;

            foreach (T item in dataList)
            {
                double columnValue = (double)columnSelector(item);
                variance += Math.Pow(columnValue - average, 2);
            }

            finalValue = Math.Sqrt(variance / count);
            return finalValue;
        }
    }
}
