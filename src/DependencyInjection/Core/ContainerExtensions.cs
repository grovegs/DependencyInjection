namespace DependencyInjection.Core;

public static class ContainerExtensions
{
    public static IContainer AddChild(this IContainer container, string name, IInstaller installer)
    {
        return Container.Create(name, container, installer);
    }
}
