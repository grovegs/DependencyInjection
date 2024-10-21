using Godot;
using GroveGames.DependencyInjection;

public sealed partial class ChildInstaller : InstallerNode
{
    public override void Install(IContainerConfigurer containerConfigurer)
    {
        GD.Print("Child installed.");
    }
}
