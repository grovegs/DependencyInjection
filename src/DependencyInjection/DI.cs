using DependencyInjection.Core;

namespace DependencyInjection;

public static class DI
{
    public static IContainer CreateContainer(string name, IContainer parent, Action<IContainerConfigurer> configure)
    {
        var cache = ContainerCache.Shared;
        return Container.Create(name, parent, cache, configure);
    }

    public static IContainer Create(ReadOnlySpan<char> path, Action<IContainerConfigurer> configure)
    {
        var cache = ContainerCache.Shared;
        return Container.Create(path, cache, configure);
    }

    public static void DisposeContainer(IContainer container)
    {
        var cache = ContainerCache.Shared;
        Container.Dispose(container, cache);
    }

    public static IContainer? FindContainer(ReadOnlySpan<char> path)
    {
        var cache = ContainerCache.Shared;
        return Container.Find(path, cache);
    }
}
