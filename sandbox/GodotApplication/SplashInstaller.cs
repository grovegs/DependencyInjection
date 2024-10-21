using Godot;
using GroveGames.DependencyInjection;

public sealed partial class SplashInstaller : InstallerNode
{
    public override void Install(IContainerConfigurer containerConfigurer)
    {
        GD.Print("Splash installed.");
    }
}
