#if !NET8_0_OR_GREATER

using System.Diagnostics.CodeAnalysis;

namespace System.Collections.Frozen;

internal sealed class FrozenDictionary<TKey, TValue>
    where TKey : notnull
{
    private readonly Dictionary<TKey, TValue> _items;

    public static FrozenDictionary<TKey, TValue> Empty { get; } = new(new Dictionary<TKey, TValue>());

    internal FrozenDictionary(Dictionary<TKey, TValue> items)
    {
        _items = items;
    }

    public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        return _items.TryGetValue(key, out value);
    }
}

#endif
