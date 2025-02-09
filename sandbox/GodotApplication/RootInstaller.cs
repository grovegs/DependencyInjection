using Godot;

using GroveGames.DependencyInjection;

[GlobalClass]
public partial class RootInstaller : RootInstallerResource
{
    public override void Install(IRootContainerBuilder builder)
    {
        GD.Print("Root installed.");
    }
}