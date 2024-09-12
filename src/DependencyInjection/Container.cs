using DependencyInjection.Caching;
using DependencyInjection.Collections;
using DependencyInjection.Resolution;

namespace DependencyInjection;

internal sealed class Container : IContainer
{
    private readonly string _name;
    private readonly IContainerResolver _resolver;
    private readonly IDisposableCollection _disposables;
    private readonly IList<IContainer> _children;
    private readonly IContainer _parent;
    private bool _isDisposed;

    public string Name => _name;
    public IContainer Parent => _parent;

    public Container(string name, IContainerResolver resolver, IDisposableCollection disposables, IContainer parent)
    {
        _name = name;
        _resolver = resolver;
        _disposables = disposables;
        _children = [];
        _parent = parent;
        _isDisposed = false;
    }

    public static IContainer Create(string name, IContainer parent, IContainerCache cache, Action<IContainerConfigurer> configure)
    {
        var resolver = new ContainerResolver(parent);
        var initializables = new InitializableCollection(resolver);
        var disposables = new DisposableCollection();
        var container = new Container(name, resolver, disposables, parent);
        var configurer = new ContainerConfigurer(resolver, initializables, disposables);
        configure.Invoke(configurer);
        parent.AddChild(container);
        cache.Add(container);
        initializables.Initialize();
        return container;
    }

    public static IContainer Create(ReadOnlySpan<char> path, IContainerCache cache, Action<IContainerConfigurer> configure)
    {
        var nameSeperatorIndex = path.LastIndexOf('/');
        var name = path[(nameSeperatorIndex + 1)..].ToString();
        var parentPath = path[..nameSeperatorIndex];
        var parent = cache.Find(parentPath)!;
        return Create(name, parent, cache, configure);
    }

    public static void Dispose(IContainer container, IContainerCache cache)
    {
        container.Dispose();
        cache.Remove(container);
    }

    public static void Dispose(ReadOnlySpan<char> path, IContainerCache cache)
    {
        var container = cache.Find(path);
        Dispose(container!, cache);
    }

    public static IContainer? Find(ReadOnlySpan<char> path, IContainerCache cache)
    {
        var container = cache.Find(path);
        return container;
    }

    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        _disposables.Dispose();

        for (var i = _children.Count - 1; i >= 0; i--)
        {
            var child = _children[i];
            child.Dispose();
        }

        _resolver.Clear();
        _children.Clear();
        _parent.RemoveChild(this);
        _isDisposed = true;
    }

    private bool ContainsChild(IContainer child)
    {
        foreach (var existingChild in _children)
        {
            if (existingChild.Name.Equals(child.Name, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    public void AddChild(IContainer child)
    {
        if (ContainsChild(child))
        {
            throw new ArgumentException($"A child container with the same name already exists in the parent container. Child Name: {child.Name}, Parent Container Name: {Name}");
        }

        _children.Add(child);
    }

    public void RemoveChild(IContainer child)
    {
        _children.Remove(child);
    }

    public object Resolve(Type registrationType)
    {
        return _resolver.Resolve(registrationType);
    }
}
