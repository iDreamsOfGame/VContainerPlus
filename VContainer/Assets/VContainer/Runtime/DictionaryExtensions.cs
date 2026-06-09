using System;
using System.Collections.Generic;

namespace VContainer
{
    public static class DictionaryExtensions
    {
        public static TValue GetOrAdd<TKey, TValue>(
            this Dictionary<TKey, TValue> dict,
            TKey key,
            Func<TKey, TValue> valueFactory)
        {
            if (dict.TryGetValue(key, out var value))
                return value;
        
            value = valueFactory(key);
            dict.Add(key, value);
            return value;
        }
    }
}