using Godot;

using GroveGames.DependencyInjection;

public sealed partial class ChildInstaller : SceneInstallerNode
{
    public override void Install(IContainerBuilder builder)
    {
        GD.Print("Child installed.");
    }
}