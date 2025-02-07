using GroveGames.DependencyInjection.Caching;

namespace GroveGames.DependencyInjection;

public sealed class RootContainer : IRootContainer
{
    private readonly IContainer _container;

    public string Name => _container.Name;
    public IContainer? Parent => _container.Parent;
    public IContainerCache? Cache => _container.Cache;

    internal RootContainer(IContainer container)
    {
        _container = container;
    }

    public void AddChild(IContainer child)
    {
        _container.AddChild(child);
    }

    public void Dispose()
    {
        _container.Dispose();
    }

    public void RemoveChild(IContainer child)
    {
        _container.RemoveChild(child);
    }

    public object Resolve(Type registrationType)
    {
        return _container.Resolve(registrationType);
    }
}
