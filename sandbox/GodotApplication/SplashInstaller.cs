using Godot;

using GroveGames.DependencyInjection;

public sealed partial class SplashInstaller : SceneInstallerNode
{
    public override void Install(IContainerBuilder builder)
    {
        GD.Print("Splash installed.");
    }
}