using GroveGames.DependencyInjection.Caching;
using GroveGames.DependencyInjection.Collections;
using GroveGames.DependencyInjection.Resolution;

namespace GroveGames.DependencyInjection;

public sealed class RootContainer : IContainer
{
    private readonly Container _container;

    public string Name => _container.Name;
    public IContainer Parent => _container.Parent;

    internal RootContainer(IContainerCache cache, Action<IContainerConfigurer> configure)
    {
        var name = string.Empty;
        var parent = new EmptyContainer();
        var resolver = new RootContainerResolver();
        var disposables = new DisposableCollection();
        _container = new Container(name, parent, resolver, cache, disposables, configure);
    }

    public static RootContainer Create(Action<IContainerConfigurer> configure)
    {
        var cache = ContainerCache.Shared;
        return new RootContainer(cache, configure);
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