namespace DependencyInjection.Core;

public static class ContainerExtensions
{
    public static IContainer AddChild(this IContainer container, string name, IContainerCache cache, Action<IContainerConfigurer> configurer)
    {
        return DependencyInjection.Container.Create(name, container, cache, configurer);
    }

    public static IContainer AddChild(this IContainer container, string name, IContainerCache cache, IInstaller installer)
    {
        return DependencyInjection.Container.Create(name, container, cache, installer.Install);
    }

    public static IContainer AddChild(this IContainer container, string name, Action<IContainerConfigurer> configurer)
    {
        var cache = ContainerCache.Shared;
        return AddChild(container, name, cache, configurer);
    }

    public static IContainer AddChild(this IContainer container, string name, IInstaller installer)
    {
        var cache = ContainerCache.Shared;
        return AddChild(container, name, cache, installer);
    }
}
