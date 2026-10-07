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
    {
        return builder.AddSingleton(typeof(TService), (object)instance!);
    }

    public static IContainerBuilder AddSingleton<TService>(this IContainerBuilder builder, Func<IObjectResolver, TService> factory)
    {
        ArgumentNullException.ThrowIfNull(factory);
        return builder.AddSingleton(typeof(TService), ToObjectFactory(factory));
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
    {
        ArgumentNullException.ThrowIfNull(factory);
        return builder.AddTransient(typeof(TService), ToObjectFactory(factory));
    }

    private static Func<IObjectResolver, object> ToObjectFactory<TService>(Func<IObjectResolver, TService> factory)
    {
        if (factory is Func<IObjectResolver, object> objectFactory)
        {
            return objectFactory;
        }

        return resolver => factory.Invoke(resolver)!;
    }
}
