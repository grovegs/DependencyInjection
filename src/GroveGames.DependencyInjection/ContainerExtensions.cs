namespace GroveGames.DependencyInjection;

public static class ContainerExtensions
{
    public static IContainer CreateChild(this IContainer container, IInstaller installer)
    {
        ArgumentNullException.ThrowIfNull(installer);
        return container.CreateChild(installer.Install);
    }
}
