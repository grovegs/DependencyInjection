using Godot;

using GroveGames.DependencyInjection;

public sealed partial class ChildInstaller : SceneInstaller
{
    public override void Install(IContainerBuilder builder)
    {
        GD.Print("Child installed.");
    }
}
