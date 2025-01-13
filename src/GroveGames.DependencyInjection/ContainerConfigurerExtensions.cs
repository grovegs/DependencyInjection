using System.Diagnostics.CodeAnalysis;

namespace GroveGames.DependencyInjection;

public static class ContainerConfigurerExtensions
{
    public static void AddSingleton(this IContainerConfigurer containerConfigurer, object implementationInstance)
    {
        var implementationType = implementationInstance.GetType();
        containerConfigurer.AddSingleton(implementationType, implementationInstance);
    }

    public static void AddSingleton(this IContainerConfigurer containerConfigurer, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors)] Type implementationType)
    {
        containerConfigurer.AddSingleton(implementationType, implementationType);
    }

    public static void AddTransient(this IContainerConfigurer containerConfigurer, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors)] Type implementationType)
    {
        containerConfigurer.AddTransient(implementationType, implementationType);
    }

    public static void AddSingleton<TRegistration, TImplementation>(this IContainerConfigurer containerConfigurer, TImplementation implementationInstance)
        where TRegistration : class
        where TImplementation : class, TRegistration
    {
        var registrationType = typeof(TRegistration);
        containerConfigurer.AddSingleton(registrationType, implementationInstance);
    }

    public static void AddSingleton<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors)] TRegistration, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors)] TImplementation>(this IContainerConfigurer containerConfigurer)
        where TRegistration : class
        where TImplementation : class, TRegistration
    {
        var registrationType = typeof(TRegistration);
        var implementationType = typeof(TImplementation);
        containerConfigurer.AddSingleton(registrationType, implementationType);
    }

    public static void AddSingleton<TImplementation>(this IContainerConfigurer containerConfigurer, TImplementation implementationInstance)
        where TImplementation : class
    {
        var implementationType = typeof(TImplementation);
        containerConfigurer.AddSingleton(implementationType, implementationInstance);
    }

    public static void AddSingleton<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors)] TImplementation>(this IContainerConfigurer containerConfigurer)
        where TImplementation : class
    {
        var implementationType = typeof(TImplementation);
        containerConfigurer.AddSingleton(implementationType, implementationType);
    }

    public static void AddTransient<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors)] TRegistration, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors)] TImplementation>(this IContainerConfigurer containerConfigurer)
        where TRegistration : class
        where TImplementation : class, TRegistration
    {
        var registrationType = typeof(TRegistration);
        var implementationType = typeof(TImplementation);
        containerConfigurer.AddTransient(registrationType, implementationType);
    }

    public static void AddTransient<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors)] TImplementation>(this IContainerConfigurer containerConfigurer)
        where TImplementation : class
    {
        var implementationType = typeof(TImplementation);
        containerConfigurer.AddTransient(implementationType, implementationType);
    }
}