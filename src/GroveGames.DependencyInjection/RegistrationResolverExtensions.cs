using GroveGames.DependencyInjection.Resolution;

namespace GroveGames.DependencyInjection;

public static class RegistrationResolverExtensions
{
    public static T Resolve<T>(this IRegistrationResolver resolver)
    where T : class
    {
        var type = typeof(T);
        return resolver.Resolve(type) is not T implementationInstance
            ? throw new RegistrationNotFoundException(type)
            : implementationInstance;
    }
}
