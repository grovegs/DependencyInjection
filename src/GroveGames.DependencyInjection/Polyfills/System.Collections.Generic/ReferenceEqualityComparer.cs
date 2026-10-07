#if !NET5_0_OR_GREATER

using System.Runtime.CompilerServices;

namespace System.Collections.Generic;

internal sealed class ReferenceEqualityComparer : IEqualityComparer<object?>
{
    public static ReferenceEqualityComparer Instance { get; } = new();

    private ReferenceEqualityComparer()
    {
    }

    public new bool Equals(object? x, object? y)
    {
        return ReferenceEquals(x, y);
    }

    public int GetHashCode(object? obj)
    {
        return RuntimeHelpers.GetHashCode(obj!);
    }
}

#endif
