using System.Diagnostics.CodeAnalysis;

using GroveGames.DependencyInjection.Caching;
using GroveGames.DependencyInjection.Collections;
using GroveGames.DependencyInjection.Resolution;

namespace GroveGames.DependencyInjection;

internal sealed class ContainerBuilder : IContainerBuilder
{
    private readonly string _name;
    private readonly IContainer _parent;
    private readonly IContainerResolver _resolver;
    private readonly IContainerCache _cache;
    private readonly IDisposableCollection _disposables;

    public ContainerBuilder(string name, IContainer parent, IContainerResolver resolver, IContainerCache cache)
    {
        _name = name;
        _parent = parent;
        _resolver = resolver;
        _cache = cache;
        _disposables = new DisposableCollection();
    }

    public Container Build()
    {
        var container = new Container(_name, _parent, _resolver, _cache, _disposables);
        AddSingleton(typeof(IObjectResolver), container);
        return container;
    }

    private void AddSingleton(Type registrationType, Type implementationType, IInstanceResolver resolver)
    {
        var singletonResolver = new SingletonResolver(resolver);
        _resolver.AddInstanceResolver(registrationType, singletonResolver);
    }

    public IContainerBuilder AddSingleton(Type registrationType, object implementationInstance)
    {
        var objectResolver = new InitializedObjectResolver(implementationInstance, _resolver, _disposables);
        AddSingleton(registrationType, registrationType, objectResolver);
        return this;
    }

    public IContainerBuilder AddSingleton([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors)] Type registrationType, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors)] Type implementationType)
    {
        var objectResolver = new UninitializedObjectResolver(implementationType, _resolver, _disposables);
        AddSingleton(registrationType, implementationType, objectResolver);
        return this;
    }

    public IContainerBuilder AddSingleton(Type registrationType, Func<object> factory)
    {
        var objectResolver = new FactoryObjectResolver(factory, _resolver, _disposables);
        AddSingleton(registrationType, registrationType, objectResolver);
        return this;
    }

    private void AddTransient(Type registrationType, Type implementationType, IInstanceResolver resolver)
    {
        var instanceResolver = new TransientResolver(resolver);
        _resolver.AddInstanceResolver(registrationType, instanceResolver);
    }

    public IContainerBuilder AddTransient([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors)] Type registrationType, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors)] Type implementationType)
    {
        var objectResolver = new UninitializedObjectResolver(implementationType, _resolver, _disposables);
        AddTransient(registrationType, implementationType, objectResolver);
        return this;
    }

    public IContainerBuilder AddTransient(Type registrationType, Func<object> factory)
    {
        var objectResolver = new FactoryObjectResolver(factory, _resolver, _disposables);
        AddTransient(registrationType, registrationType, objectResolver);
        return this;
    }
}
