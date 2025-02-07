using GroveGames.DependencyInjection.Caching;
using GroveGames.DependencyInjection.Collections;
using GroveGames.DependencyInjection.Resolution;

namespace GroveGames.DependencyInjection;

public sealed class Container : IContainer
{
    private readonly string _name;
    private readonly IContainer _parent;
    private readonly IContainerResolver _resolver;
    private readonly IContainerCache _cache;
    private readonly IDisposableCollection _disposables;
    private readonly List<IContainer> _children;
    private bool _isDisposed;

    public string Name => _name;
    public IContainer Parent => _parent;
    public IContainerCache Cache => _cache;

    internal Container(string name, IContainer parent, IContainerResolver resolver, IContainerCache cache, IDisposableCollection disposables)
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
