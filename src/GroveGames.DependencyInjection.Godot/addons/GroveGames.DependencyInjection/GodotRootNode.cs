using Godot;

namespace GroveGames.DependencyInjection;

public sealed partial class GodotRootNode : Node
{
    public override void _Ready()
    {
        var window = GetTree().Root;
        var godotRoot = new GodotRoot(window);
        godotRoot.Run();
    }
}
