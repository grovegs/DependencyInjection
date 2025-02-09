using GroveGames.DependencyInjection.Resolution;

namespace GroveGames.DependencyInjection;

public static class ContainerFactory
{
    public static Container CreateContainer(string name, IContainer parent, Action<IContainerBuilder> configure)
    {
        var resolver = new ContainerResolver(parent);
        var cache = parent.Cache!;
        var builder = new ContainerBuilder(name, parent, resolver, cache);
        configure.Invoke(builder);
        return builder.Build();
    }
}
