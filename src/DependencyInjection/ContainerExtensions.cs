using DependencyInjection.Core;

namespace DependencyInjection;

public static class ContainerExtensions
{
    public static IContainer AddChild(this IContainer container, string name, IContainerCache cache, Action<IContainerConfigurer> configure)
    {
        return Container.Create(name, container, cache, configure);
    }

    public static IContainer AddChild(this IContainer container, string name, IContainerCache cache, IInstaller installer)
    {
        return Container.Create(name, container, cache, installer.Install);
    }

    public static IContainer AddChild(this IContainer container, string name, Action<IContainerConfigurer> configure)
    {
        var cache = ContainerCache.Shared;
        return AddChild(container, name, cache, configure);
    }

    public static IContainer AddChild(this IContainer container, string name, IInstaller installer)
    {
        var cache = ContainerCache.Shared;
        return AddChild(container, name, cache, installer);
    }
}
