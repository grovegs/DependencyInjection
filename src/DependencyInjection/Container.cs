using DependencyInjection.Core;
using DependencyInjection.Resolution;

namespace DependencyInjection;

public static class Container
{
    internal static IContainer Create(string name, IContainer parent, IContainerCache cache, Action<IContainerConfigurer> configure)
    {
        var resolver = new ContainerResolver(parent);
        var initializables = new InitializableCollection(resolver);
        var disposables = new DisposableCollection();
        var container = new Core.Container(name, resolver, disposables, parent);
        var configurer = new ContainerConfigurer(resolver, initializables, disposables);
        configure.Invoke(configurer);
        parent.AddChild(container);
        cache.Add(container);
        initializables.Initialize();
        return container;
    }

    internal static IContainer Create(ReadOnlySpan<char> path, IContainerCache cache, Action<IContainerConfigurer> configure)
    {
        var nameSeperatorIndex = path.LastIndexOf('/');
        var name = path[(nameSeperatorIndex + 1)..].ToString();
        var parentPath = path[..nameSeperatorIndex];
        var parent = cache.Find(parentPath)!;
        return Create(name, parent, cache, configure);
    }

    internal static void Dispose(IContainer container, IContainerCache cache)
    {
        container.Dispose();
        cache.Remove(container);
    }

    internal static void Dispose(ReadOnlySpan<char> path, IContainerCache cache)
    {
        var container = cache.Find(path);
        Dispose(container!, cache);
    }

    internal static IContainer? Find(ReadOnlySpan<char> path, IContainerCache cache)
    {
        var container = cache.Find(path);
        return container;
    }

    public static IContainer Create(string name, IContainer parent, Action<IContainerConfigurer> configure)
    {
        var cache = ContainerCache.Shared;
        return Create(name, parent, cache, configure);
    }

    public static IContainer Create(ReadOnlySpan<char> path, Action<IContainerConfigurer> configure)
    {
        var cache = ContainerCache.Shared;
        return Create(path, cache, configure);
    }

    public static void Dispose(IContainer container)
    {
        var cache = ContainerCache.Shared;
        Dispose(container, cache);
    }

    public static IContainer? Find(ReadOnlySpan<char> path)
    {
        var cache = ContainerCache.Shared;
        return Find(path, cache);
    }
}
