using GroveGames.DependencyInjection.Caching;
using GroveGames.DependencyInjection.Collections;
using GroveGames.DependencyInjection.Resolution;

namespace GroveGames.DependencyInjection;

public sealed class Container : IContainer
{
    private readonly string _name;
    private readonly IContainerResolver _resolver;
    private readonly IContainerCache _cache;
    private readonly IDisposableCollection _disposables;
    private readonly List<IContainer> _children;
    private readonly IContainer _parent;
    private bool _isDisposed;

    public string Name => _name;
    public IContainer Parent => _parent;

    internal Container(string name, IContainer parent, IContainerResolver resolver, IContainerCache cache, IDisposableCollection disposables, Action<IContainerConfigurer> configure)
    {
        _name = name;
        _parent = parent;
        _resolver = resolver;
        _cache = cache;
        _disposables = disposables;
        _children = [];
        _isDisposed = false;
        _parent.AddChild(this);
        _cache.Add(this);
        var initializables = new InitializableCollection(resolver);
        var configurer = new ContainerConfigurer(resolver, initializables, disposables);
        configure.Invoke(configurer);
        initializables.Initialize();
    }

    internal static Container Create(string name, IContainer parent, IContainerResolver resolver, IContainerCache cache, Action<IContainerConfigurer> configure)
    {
        var disposables = new DisposableCollection();
        var container = new Container(name, parent, resolver, cache, disposables, configure);
        return container;
    }

    internal static Container Create(string name, IContainer parent, IContainerCache cache, Action<IContainerConfigurer> configure)
    {
        var resolver = new ContainerResolver(parent);
        return Create(name, parent, resolver, cache, configure);
    }

    internal static Container Create(in ReadOnlySpan<char> path, IContainerCache cache, Action<IContainerConfigurer> configure)
    {
        var nameSeperatorIndex = path.LastIndexOf('/');
        var name = path[(nameSeperatorIndex + 1)..].ToString();
        var parentPath = nameSeperatorIndex == -1 ? string.Empty : path[..nameSeperatorIndex];
        var parent = cache.Find(parentPath)!;
        return Create(name, parent, cache, configure);
    }

    public static Container Create(string name, IContainer parent, Action<IContainerConfigurer> configure)
    {
        var cache = ContainerCache.Shared;
        return Create(name, parent, cache, configure);
    }

    public static Container Create(in ReadOnlySpan<char> path, Action<IContainerConfigurer> configure)
    {
        var cache = ContainerCache.Shared;
        return Create(path, cache, configure);
    }

    internal static void Dispose(in ReadOnlySpan<char> path, IContainerCache cache)
    {
        var container = cache.Find(path);
        container?.Dispose();
    }

    public static void Dispose(in ReadOnlySpan<char> path)
    {
        var container = Find(path);
        container?.Dispose();
    }

    internal static IContainer? Find(in ReadOnlySpan<char> path, IContainerCache cache)
    {
        var container = cache.Find(path);
        return container;
    }

    public static IContainer? Find(in ReadOnlySpan<char> path)
    {
        var cache = ContainerCache.Shared;
        return Find(path, cache);
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
        _cache.Remove(this);
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
