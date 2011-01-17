using System;
using System.Collections.Generic;

namespace ContinuousLinq
{
    /// <summary>
    /// Many thanks to Oren @ the SLINQ project, this class is extremely useful
    /// and a huge time-saver. This class allows us to encapsulate a sort clause
    /// from a LINQ query in a single instance that can be passed as a parameter
    /// or, in the case of the SortingViewAdapter, stored as an instance member variable.
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    public class FuncComparer<TSource, TKey> : Comparer<TSource>
           where TKey : IComparable
    {
        private readonly Func<TSource, TKey> keyFunc;
        private readonly int multiplier = 1;

        public FuncComparer(Func<TSource, TKey> keyFunc, bool descending)
        {
            this.keyFunc = keyFunc;

            if (descending)
                multiplier = -1;
            else
                multiplier = 1;
        }

        public override int Compare(TSource x, TSource y)
        {
            return multiplier * keyFunc(x).CompareTo(keyFunc(y));
        }
    }
}
