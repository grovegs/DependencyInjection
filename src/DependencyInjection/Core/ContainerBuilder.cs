using DependencyInjection.Resolution;

namespace DependencyInjection.Core;

internal sealed class ContainerBuilder : IContainerBuilder
{
    private readonly ContainerResolver _containerResolver;
    private readonly InitializableCollection _initializableCollection;
    private readonly DisposableCollection _disposableCollection;
    private readonly List<IContainer> _children;
    private readonly string _name;
    private readonly IContainer _parent;

    public ContainerBuilder(string name, IContainer parent)
    {
        _containerResolver = new ContainerResolver(parent);
        _initializableCollection = new InitializableCollection(_containerResolver);
        _disposableCollection = new DisposableCollection();
        _children = [];
        _name = name;
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
        var container = new Container(_name, _containerResolver, _disposableCollection, _children, _parent);
        _parent?.AddChild(container);
        _initializableCollection.Initialize();
        return container;
    }
}
