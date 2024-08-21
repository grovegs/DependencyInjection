using DependencyInjection.Resolution;

namespace DependencyInjection.Core;

internal sealed class Container : IContainer
{
    private readonly IContainerResolver _resolver;
    private readonly IDisposableCollection _disposables;
    private readonly IList<IContainer> _children;
    private readonly IContainer _parent;

    public Container(IContainerResolver resolver, IDisposableCollection disposables, IList<IContainer> children, IContainer parent)
    {
        _resolver = resolver;
        _disposables = disposables;
        _children = children;
        _parent = parent;
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
        _parent.RemoveChild(this);
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
