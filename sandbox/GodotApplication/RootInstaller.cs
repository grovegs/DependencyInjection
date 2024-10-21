using GroveGames.DependencyInjection;
using Godot;

[GlobalClass]
public partial class RootInstaller : RootInstallerResource
{
    public override void Install(IContainerConfigurer containerConfigurer)
    {
        GD.Print("Root installed.");
    }
}
