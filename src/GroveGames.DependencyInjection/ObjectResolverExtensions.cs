using System.Diagnostics.CodeAnalysis;

namespace GroveGames.DependencyInjection;

public static class ObjectResolverExtensions
{
    public static T Resolve<T>(this IObjectResolver resolver)
    {
        return (T)resolver.Resolve(typeof(T));
    }

    public static bool TryResolve<T>(this IObjectResolver resolver, [MaybeNullWhen(false)] out T instance)
    {
        if (resolver.TryResolve(typeof(T), out var resolved))
        {
            instance = (T)resolved;
            return true;
        }

        instance = default;
        return false;
    }
}
