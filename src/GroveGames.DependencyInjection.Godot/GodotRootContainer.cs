using Godot;

using GroveGames.DependencyInjection.Caching;

namespace GroveGames.DependencyInjection;

public sealed partial class GodotRootContainer : Node, IRootContainer
{
    private readonly IRootContainer _container;

    public IContainer Parent => _container.Parent;

    public IContainerCache Cache => _container.Cache;

    public new string Name => _container.Name;

    public GodotRootContainer(IRootContainer container) : base()
    {
        base.Name = "RootContainer";
        _container = container;
    }

    public object Resolve(Type registrationType)
    {
        return _container.Resolve(registrationType);
    }

    public void AddChild(IContainer child)
    {
        _container.AddChild(child);
    }

    public void RemoveChild(IContainer child)
    {
        _container.RemoveChild(child);
    }

    public IContainer? FindChild(ReadOnlySpan<char> path)
    {
        return _container.FindChild(path);
    }
}
