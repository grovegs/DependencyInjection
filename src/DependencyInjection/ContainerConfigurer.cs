using DependencyInjection.Collections;
using DependencyInjection.Resolution;

namespace DependencyInjection;

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

    public void AddInstance(Type registrationType, object implementationInstance)
    {
        var instanceResolver = new InstanceResolver(implementationInstance);
        _containerResolver.AddInstanceResolver(registrationType, instanceResolver);
    }

    public void AddSingleton(Type registrationType, Type implementationType)
    {
        var objectResolver = new ObjectResolver(implementationType, _containerResolver, _disposableCollection);
        var instanceResolver = new SingletonResolver(objectResolver);
        _containerResolver.AddInstanceResolver(registrationType, instanceResolver);
        _initializableCollection.TryAdd(registrationType, implementationType);
    }

    public void AddTransient(Type registrationType, Type implementationType)
    {
        var objectResolver = new ObjectResolver(implementationType, _containerResolver, _disposableCollection);
        var instanceResolver = new TransientResolver(objectResolver);
        _containerResolver.AddInstanceResolver(registrationType, instanceResolver);
        _initializableCollection.TryAdd(registrationType, implementationType);
    }
}
