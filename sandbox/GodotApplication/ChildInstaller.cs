using Godot;

using GroveGames.DependencyInjection;

public sealed partial class ChildInstaller : SceneInstallerNode
{
    public override void Install(IContainerConfigurer containerConfigurer)
    {
        GD.Print("Child installed.");
    }
}