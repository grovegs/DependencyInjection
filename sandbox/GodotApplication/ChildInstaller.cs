using Godot;

using GroveGames.DependencyInjection;
using GroveGames.DependencyInjection.Godot;

public sealed partial class ChildInstaller : SceneInstaller
{
    public override void Install(IContainerBuilder builder)
    {
        GD.Print("Child installed.");
    }
}
