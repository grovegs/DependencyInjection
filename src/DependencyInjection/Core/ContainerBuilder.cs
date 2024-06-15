using DependencyInjection.Resolution;

namespace DependencyInjection.Core;

public sealed class ContainerBuilder : IContainerBuilder
{
    private readonly IContainerResolver _containerResolver;
    private readonly IInitializableCollection _initializableCollection;
    private readonly IDisposableCollection _disposableCollection;
    private readonly IList<IContainer> _children;
    private readonly IContainer _parent;

    public ContainerBuilder(IContainer parent)
    {
        _containerResolver = new ContainerResolver(parent);
        _initializableCollection = new InitializableCollection(_containerResolver);
        _disposableCollection = new DisposableCollection();
        _children = [];
        _parent = parent;
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

    public IContainer Build()
    {
        var container = new Container(_containerResolver, _disposableCollection, _children, _parent);
        _parent.AddChild(container);
        _initializableCollection.Initialize();
        return container;
    }
}
