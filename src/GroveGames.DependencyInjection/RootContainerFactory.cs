using GroveGames.DependencyInjection.Caching;
using GroveGames.DependencyInjection.Resolution;

namespace GroveGames.DependencyInjection;

public static class RootContainerFactory
{
    public static RootContainer CreateRootContainer(Action<IRootContainerBuilder> configure)
    {
        var name = string.Empty;
        var parent = new EmptyContainer();
        var resolver = new RootContainerResolver();
        var cache = new ContainerCache();
        var builder = new RootContainerBuilder(name, parent, resolver, cache);
        configure.Invoke(builder);
        return builder.Build();
    }
}