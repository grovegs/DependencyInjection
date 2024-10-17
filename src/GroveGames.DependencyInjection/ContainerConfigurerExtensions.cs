namespace GroveGames.DependencyInjection;

public static class ContainerConfigurerExtensions
{
    public static void AddSingleton(this IContainerConfigurer containerConfigurer, Type implementationType)
    {
        containerConfigurer.AddSingleton(implementationType, implementationType);
    }

    public static void AddTransient(this IContainerConfigurer containerConfigurer, Type implementationType)
    {
        containerConfigurer.AddTransient(implementationType, implementationType);
    }

    public static void AddSingleton<TRegistration, TImplementation>(this IContainerConfigurer containerConfigurer)
        where TRegistration : class
        where TImplementation : class, TRegistration
    {
        var registrationType = typeof(TRegistration);
        var implementationType = typeof(TImplementation);
        containerConfigurer.AddSingleton(registrationType, implementationType);
    }

    public static void AddSingleton<TImplementation>(this IContainerConfigurer containerConfigurer)
        where TImplementation : class
    {
        var implementationType = typeof(TImplementation);
        containerConfigurer.AddSingleton(implementationType, implementationType);
    }

    public static void AddTransient<TRegistration, TImplementation>(this IContainerConfigurer containerConfigurer)
        where TRegistration : class
        where TImplementation : class, TRegistration
    {
        var registrationType = typeof(TRegistration);
        var implementationType = typeof(TImplementation);
        containerConfigurer.AddTransient(registrationType, implementationType);
    }

    public static void AddTransient<TImplementation>(this IContainerConfigurer containerConfigurer)
        where TImplementation : class
    {
        var implementationType = typeof(TImplementation);
        containerConfigurer.AddTransient(implementationType, implementationType);
    }
}
