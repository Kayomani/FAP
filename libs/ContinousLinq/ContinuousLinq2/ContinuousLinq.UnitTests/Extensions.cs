using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ContinuousLinq.UnitTests
{
    static class Extensions
    {
        public static TSource[] ToArray<TSource>(this IList list)
        {
            TSource[] array = new TSource[list.Count];
            for(int i = 0; i<list.Count; i++)
            {
                array[i] = (TSource)list[i];
            }
            return array;
        }
        public static TSource[] ItemToArray<TSource>(this TSource item)
        {
            return new[] {item};
        }

        public static IEnumerator<TSource> GetEnumerator<TSource>(this TSource[] array)
        {
            return array.ToList().GetEnumerator();
        }
    }
}
