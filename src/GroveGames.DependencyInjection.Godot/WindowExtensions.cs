using Godot;

namespace GroveGames.DependencyInjection;

public static class WindowExtensions
{
    public static IContainer GetContainer(this Window window, string path)
    {
        var rootContainer = window.GetNode<GodotRootContainer>("RootContainer");
        var container = rootContainer.Cache.Find(path);

        if (container == null)
        {
            GD.Print($"Container not found. Path: {path}");
        }

        return container;
    }
}
