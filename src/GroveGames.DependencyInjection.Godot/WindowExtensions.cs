using Godot;

namespace GroveGames.DependencyInjection;

public static class WindowExtensions
{
    public static IRootContainer GetContainer(this Window window)
    {
        var container = window.GetNode<GodotRootContainer>("RootContainer");
        return container;
    }
}
