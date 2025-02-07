using GroveGames.DependencyInjection.Resolution;

namespace GroveGames.DependencyInjection;

public static class RegistrationResolverExtensions
{
    public static T Resolve<T>(this IRegistrationResolver resolver)
    where T : class
    {
        return (T)resolver.Resolve(typeof(T));
    }
}
