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
    private readonly List<Type> _immediateResolutionTypes;

    public ContainerBuilder(string name, IContainer parent, IContainerResolver resolver, IContainerCache cache)
    {
        _name = name;
        _parent = parent;
        _resolver = resolver;
        _cache = cache;
        _disposables = new DisposableCollection();
        _immediateResolutionTypes = [];
    }

    public Container Build()
    {
        var container = new Container(_name, _parent, _resolver, _cache, _disposables);
        AddSingleton(typeof(IObjectResolver), container);

        foreach (var immediateResolutionType in _immediateResolutionTypes)
        {
            _resolver.Resolve(immediateResolutionType);
        }

        return container;
    }

    private void AddSingleton(Type registrationType, Type implementationType, IInstanceResolver instanceResolver)
    {
        var resolver = new SingletonResolver(instanceResolver);
        _resolver.AddResolver(registrationType, resolver);
    }

    public IContainerBuilder AddSingleton(Type registrationType, object implementationInstance)
    {
        var resolver = new InitializedObjectResolver(implementationInstance, _resolver, _disposables);
        AddSingleton(registrationType, registrationType, resolver);
        _immediateResolutionTypes.Add(registrationType);
        return this;
    }

    public IContainerBuilder AddSingleton([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors)] Type registrationType, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors)] Type implementationType)
    {
        var resolver = new UninitializedObjectResolver(implementationType, _resolver, _disposables);
        AddSingleton(registrationType, implementationType, resolver);
        return this;
    }

    public IContainerBuilder AddSingleton(Type registrationType, Func<object> instanceFactory)
    {
        var resolver = new FactoryObjectResolver(instanceFactory, _resolver, _disposables);
        AddSingleton(registrationType, registrationType, resolver);
        return this;
    }

    private void AddTransient(Type registrationType, Type implementationType, IInstanceResolver instanceResolver)
    {
        var resolver = new TransientResolver(instanceResolver);
        _resolver.AddResolver(registrationType, resolver);
    }

    public IContainerBuilder AddTransient([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors)] Type registrationType, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors)] Type implementationType)
    {
        var resolver = new UninitializedObjectResolver(implementationType, _resolver, _disposables);
        AddTransient(registrationType, implementationType, resolver);
        return this;
    }

    public IContainerBuilder AddTransient(Type registrationType, Func<object> instanceFactory)
    {
        var resolver = new FactoryObjectResolver(instanceFactory, _resolver, _disposables);
        AddTransient(registrationType, registrationType, resolver);
        return this;
    }
}
