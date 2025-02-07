using System.Diagnostics.CodeAnalysis;

namespace GroveGames.DependencyInjection;

public static class ContainerBuilderExtensions
{
    public static IContainerBuilder AddSingleton(this IContainerBuilder builder, object implementationInstance)
    {
        var implementationType = implementationInstance.GetType();
        return builder.AddSingleton(implementationType, implementationInstance);
    }

    public static IContainerBuilder AddSingleton(this IContainerBuilder builder, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors)] Type implementationType)
    {
        return builder.AddSingleton(implementationType, implementationType);
    }

    public static IContainerBuilder AddSingleton<TRegistration, TImplementation>(this IContainerBuilder builder, TImplementation implementationInstance)
        where TRegistration : class
        where TImplementation : class, TRegistration
    {
        var registrationType = typeof(TRegistration);
        return builder.AddSingleton(registrationType, implementationInstance);
    }

    public static IContainerBuilder AddSingleton<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors)] TRegistration, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors)] TImplementation>(this IContainerBuilder builder)
        where TRegistration : class
        where TImplementation : class, TRegistration
    {
        var registrationType = typeof(TRegistration);
        var implementationType = typeof(TImplementation);
        return builder.AddSingleton(registrationType, implementationType);
    }

    public static IContainerBuilder AddSingleton<TImplementation>(this IContainerBuilder builder, TImplementation implementationInstance)
        where TImplementation : class
    {
        var implementationType = typeof(TImplementation);
        return builder.AddSingleton(implementationType, implementationInstance);
    }

    public static IContainerBuilder AddSingleton<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors)] TImplementation>(this IContainerBuilder builder)
        where TImplementation : class
    {
        var implementationType = typeof(TImplementation);
        return builder.AddSingleton(implementationType, implementationType);
    }

    public static IContainerBuilder AddSingleton<TRegistration>(this IContainerBuilder builder, Func<TRegistration> factory)
        where TRegistration : class
    {
        var registrationType = typeof(TRegistration);
        return builder.AddSingleton(registrationType, factory);
    }

    public static IContainerBuilder AddTransient(this IContainerBuilder builder, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors)] Type implementationType)
    {
        return builder.AddTransient(implementationType, implementationType);
    }

    public static IContainerBuilder AddTransient<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors)] TRegistration, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors)] TImplementation>(this IContainerBuilder builder)
        where TRegistration : class
        where TImplementation : class, TRegistration
    {
        var registrationType = typeof(TRegistration);
        var implementationType = typeof(TImplementation);
        return builder.AddTransient(registrationType, implementationType);
    }

    public static IContainerBuilder AddTransient<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors)] TImplementation>(this IContainerBuilder builder)
        where TImplementation : class
    {
        var implementationType = typeof(TImplementation);
        return builder.AddTransient(implementationType, implementationType);
    }

    public static IContainerBuilder AddTransient<TRegistration>(this IContainerBuilder builder, Func<TRegistration> factory)
    where TRegistration : class
    {
        var registrationType = typeof(TRegistration);
        return builder.AddTransient(registrationType, factory);
    }
}