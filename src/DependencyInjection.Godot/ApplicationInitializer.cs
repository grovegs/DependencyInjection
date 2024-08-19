using DependencyInjection.Core;
using Godot;
using Container = DependencyInjection.Core.Container;

namespace DependencyInjection.Godot;

public sealed class ApplicationInitializer : IDisposable
{
    public ApplicationInitializer()
    {
        var containerBuilder = new ContainerBuilder(Container.Root);
        var installerScene = ResourceLoader.Load<PackedScene>("res://ApplicationInstaller.tscn");
        var installer = installerScene.Instantiate<IInstaller>();
        installer?.Install(containerBuilder);
        var applicationContainer = containerBuilder.Build();
        ApplicationContainerProvider.Set(applicationContainer);
        // TODO: Destroy installer
    }

    public void Dispose()
    {
        var applicationContainer = ApplicationContainerProvider.Get();
        applicationContainer.Dispose();
    }
}

public interface IA
{

}

public class A : IA
{

}
