using DependencyInjection.Resolution;

namespace DependencyInjection.Core;

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

    internal Container(string name, IContainerResolver resolver, IDisposableCollection disposables, IContainer parent)
    {
        _name = name;
        _resolver = resolver;
        _disposables = disposables;
        _children = [];
        _parent = parent;
        _isDisposed = false;
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

    public void AddChild(IContainer child)
    {
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
