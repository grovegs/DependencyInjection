using System.Diagnostics.CodeAnalysis;

namespace GroveGames.DependencyInjection;

public static class ContainerBuilderExtensions
{
    public static IContainerBuilder AddSingleton<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TImplementation>(this IContainerBuilder builder)
        where TImplementation : class
    {
        return builder.AddSingleton(typeof(TImplementation), typeof(TImplementation));
    }

    public static IContainerBuilder AddSingleton<TService, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TImplementation>(this IContainerBuilder builder)
        where TService : class
        where TImplementation : class, TService
    {
        return builder.AddSingleton(typeof(TService), typeof(TImplementation));
    }

    public static IContainerBuilder AddSingleton<TService>(this IContainerBuilder builder, TService instance)
        where TService : class
    {
        return builder.AddSingleton(typeof(TService), (object)instance);
    }

    public static IContainerBuilder AddSingleton<TService>(this IContainerBuilder builder, Func<IObjectResolver, TService> factory)
        where TService : class
    {
        ArgumentNullException.ThrowIfNull(factory);
        return builder.AddSingleton(typeof(TService), factory);
    }

    public static IContainerBuilder AddTransient<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TImplementation>(this IContainerBuilder builder)
        where TImplementation : class
    {
        return builder.AddTransient(typeof(TImplementation), typeof(TImplementation));
    }

    public static IContainerBuilder AddTransient<TService, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TImplementation>(this IContainerBuilder builder)
        where TService : class
        where TImplementation : class, TService
    {
        return builder.AddTransient(typeof(TService), typeof(TImplementation));
    }

    public static IContainerBuilder AddTransient<TService>(this IContainerBuilder builder, Func<IObjectResolver, TService> factory)
        where TService : class
    {
        ArgumentNullException.ThrowIfNull(factory);
        return builder.AddTransient(typeof(TService), factory);
    }
}
