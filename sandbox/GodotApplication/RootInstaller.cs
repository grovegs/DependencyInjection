using Godot;

using GroveGames.DependencyInjection;

[GlobalClass]
public partial class RootInstaller : RootInstallerResource
{
    public override void Install(IContainerConfigurer containerConfigurer)
    {
        GD.Print("Root installed.");
    }
}