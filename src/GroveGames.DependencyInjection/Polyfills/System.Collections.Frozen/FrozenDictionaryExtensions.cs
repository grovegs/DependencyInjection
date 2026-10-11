#if !NET8_0_OR_GREATER

namespace System.Collections.Frozen;

internal static class FrozenDictionaryExtensions
{
    extension<TKey, TValue>(IEnumerable<KeyValuePair<TKey, TValue>> source)
        where TKey : notnull
    {
        public FrozenDictionary<TKey, TValue> ToFrozenDictionary()
        {
            return new FrozenDictionary<TKey, TValue>(new Dictionary<TKey, TValue>(source));
        }
    }
}

#endif
