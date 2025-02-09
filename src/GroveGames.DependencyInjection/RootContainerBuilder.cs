using System.Diagnostics.CodeAnalysis;

using GroveGames.DependencyInjection.Caching;
using GroveGames.DependencyInjection.Resolution;

namespace GroveGames.DependencyInjection;

internal sealed class RootContainerBuilder : IRootContainerBuilder
{
    private readonly ContainerBuilder _builder;

    public RootContainerBuilder(string name, IContainer parent, IContainerResolver resolver, IContainerCache cache)
    {
        _builder = new ContainerBuilder(name, parent, resolver, cache);
    }

    public RootContainer Build()
    {
        var container = _builder.Build();
        return new RootContainer(container);
    }

    public IContainerBuilder AddSingleton(Type registrationType, object implementationInstance)
    {
        return _builder.AddSingleton(registrationType, implementationInstance);
    }

    public IContainerBuilder AddSingleton([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors)] Type registrationType, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors)] Type implementationType)
    {
        return _builder.AddSingleton(registrationType, implementationType);
    }

    public IContainerBuilder AddSingleton(Type registrationType, Func<object> instanceFactory)
    {
        return _builder.AddSingleton(registrationType, instanceFactory);
    }

    public IContainerBuilder AddTransient([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors)] Type registrationType, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors)] Type implementationType)
    {
        return _builder.AddTransient(registrationType, implementationType);
    }

    public IContainerBuilder AddTransient(Type registrationType, Func<object> instanceFactory)
    {
        return _builder.AddTransient(registrationType, instanceFactory);
    }
}
