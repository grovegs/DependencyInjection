using DependencyInjection.Core;
using Godot;

namespace DependencyInjection.Godot;

public sealed partial class ApplicationInitializer : Node
{
    public override void _EnterTree()
    {
        var containerBuilder = new ContainerBuilder(Containers.Root);
        var installerScene = ResourceLoader.Load<PackedScene>("res://ApplicationInstaller.tscn");
        var installer = installerScene.Instantiate<NodeInstaller>();
        installer.Install(containerBuilder);
        var applicationContainer = containerBuilder.Build();
        ApplicationContainerProvider.Set(applicationContainer);
        installer.Free();
    }

    public override void _ExitTree()
    {
        var applicationContainer = ApplicationContainerProvider.Get();
        applicationContainer.Dispose();
    }

    public override void _Notification(int what)
    {
        GD.Print(what);
    }
}
