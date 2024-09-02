using DependencyInjection.Resolution;

namespace DependencyInjection.Core;

public sealed class Container : IContainer
{
    public static readonly IContainer Empty = new NullContainer();

    private readonly string _name;
    private readonly IContainerResolver _resolver;
    private readonly IDisposableCollection _disposables;
    private readonly IList<IContainer> _children;
    private readonly IContainer _parent;

    public string Name => _name;
    public IContainer Parent => _parent;

    internal Container(string name, IContainerResolver resolver, IDisposableCollection disposables, IList<IContainer> children, IContainer parent)
    {
        _name = name;
        _resolver = resolver;
        _disposables = disposables;
        _children = children;
        _parent = parent;
    }

    public static IContainer Create(string name, IContainer parent, IInstaller installer)
    {
        var builder = new ContainerBuilder(name, parent);
        installer.Install(builder);
        var container = builder.Build();
        ContainerCache.Add(container);
        return container;
    }

    public static void Dispose(IContainer container)
    {
        container.Dispose();
        ContainerCache.Remove(container);
    }

    public static IContainer? Find(ReadOnlySpan<char> path)
    {
        var container = ContainerCache.Find(path);
        return container;
    }

    public IContainer? Create(string name, IInstaller installer)
    {
        var container = Create(name, _parent, installer);
        return container;
    }

    public void Dispose()
    {
        _disposables.Dispose();

        for (var i = _children.Count - 1; i >= 0; i--)
        {
            var child = _children[i];
            child.Dispose();
        }

        _resolver.Clear();
        _children.Clear();
        _parent?.RemoveChild(this);
    }

    public void AddChild(IContainer child)
    {
        _children.Add(child);
    }

    public void RemoveChild(IContainer child)
    {
        _children.Remove(child);
    }

    public object? Resolve(Type registrationType)
    {
        return _resolver.Resolve(registrationType);
    }
}
