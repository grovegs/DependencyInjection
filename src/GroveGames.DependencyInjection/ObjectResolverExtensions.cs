using System.Diagnostics.CodeAnalysis;

namespace GroveGames.DependencyInjection;

public static class ObjectResolverExtensions
{
    public static T Resolve<T>(this IObjectResolver resolver)
        where T : class
    {
        return (T)resolver.Resolve(typeof(T));
    }

    public static bool TryResolve<T>(this IObjectResolver resolver, [NotNullWhen(true)] out T? instance)
        where T : class
    {
        if (resolver.TryResolve(typeof(T), out var resolved))
        {
            instance = (T)resolved;
            return true;
        }

        instance = null;
        return false;
    }
}
