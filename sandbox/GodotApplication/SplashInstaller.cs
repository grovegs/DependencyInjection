using Godot;

using GroveGames.DependencyInjection;

public sealed partial class SplashInstaller : SceneInstallerNode
{
    public override void Install(IContainerConfigurer containerConfigurer)
    {
        GD.Print("Splash installed.");
    }
}