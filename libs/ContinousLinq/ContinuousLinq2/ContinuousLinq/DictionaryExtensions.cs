using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ContinuousLinq
{
    public static class DictionaryExtensions
    {
        public static TValue GetOrCreate<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key) where TValue : new()
        {
            return dictionary.GetOrCreate(key, () => new TValue());
        }

        public static TValue GetOrCreate<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, Func<TValue> valueFactory) 
        {
            TValue value;
            if (!dictionary.TryGetValue(key, out value))
            {
                value = valueFactory();
                dictionary[key] = value;
            }
            return value;
        }
    }
}
