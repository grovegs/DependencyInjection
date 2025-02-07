using Godot;

namespace GroveGames.DependencyInjection;

public sealed partial class RootContainerBootstrapper : Node
{
    public override void _Ready()
    {
        var container = GodotRootContainerFactory.CreateGodotRootContainer();
        QueueFree();
    }
}
