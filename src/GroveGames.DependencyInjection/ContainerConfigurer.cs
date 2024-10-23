using GroveGames.DependencyInjection.Collections;
using GroveGames.DependencyInjection.Resolution;

namespace GroveGames.DependencyInjection;

internal sealed class ContainerConfigurer : IContainerConfigurer
{
    private readonly IContainerResolver _containerResolver;
    private readonly IInitializableCollection _initializableCollection;
    private readonly IDisposableCollection _disposableCollection;

    public ContainerConfigurer(IContainerResolver containerResolver, IInitializableCollection initializableCollection, IDisposableCollection disposableCollection)
    {
        _containerResolver = containerResolver;
        _initializableCollection = initializableCollection;
        _disposableCollection = disposableCollection;
    }

    private void AddSingleton(Type registrationType, Type implementationType, IObjectResolver objectResolver)
    {
        var instanceResolver = new SingletonResolver(objectResolver);
        _containerResolver.AddInstanceResolver(registrationType, instanceResolver);
        _initializableCollection.TryAdd(registrationType, implementationType);
    }

    public void AddSingleton(Type registrationType, object implementationInstance)
    {
        var objectResolver = new InitializedObjectResolver(implementationInstance, _containerResolver, _disposableCollection);
        AddSingleton(registrationType, registrationType, objectResolver);
    }

    public void AddSingleton(Type registrationType, Type implementationType)
    {
        var objectResolver = new UninitializedObjectResolver(implementationType, _containerResolver, _disposableCollection);
        AddSingleton(registrationType, registrationType, objectResolver);
    }

    public void AddTransient(Type registrationType, Type implementationType)
    {
        var objectResolver = new UninitializedObjectResolver(implementationType, _containerResolver, _disposableCollection);
        var instanceResolver = new TransientResolver(objectResolver);
        _containerResolver.AddInstanceResolver(registrationType, instanceResolver);
        _initializableCollection.TryAdd(registrationType, implementationType);
    }
}