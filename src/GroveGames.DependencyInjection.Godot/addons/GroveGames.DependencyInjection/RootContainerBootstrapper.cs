using Godot;

namespace GroveGames.DependencyInjection;

public sealed partial class RootContainerBootstrapper : Node
{
    public override void _EnterTree()
    {
        var container = GodotRootContainerFactory.CreateGodotRootContainer();
        var scene = GetTree().CurrentScene;
        SceneInstaller.Install(scene, container);
        QueueFree();
    }
}
