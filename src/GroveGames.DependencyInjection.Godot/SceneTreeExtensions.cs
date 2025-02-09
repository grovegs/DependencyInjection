using Godot;

namespace GroveGames.DependencyInjection;

public static class SceneTreeExtensions
{
    public static IRootContainer GetRootContainer(this SceneTree sceneTree)
    {
        var window = sceneTree.Root;
        var container = window.GetNodeOrNull<GodotRootContainer>("RootContainer") ?? throw new InvalidOperationException("RootContainer not found in SceneTree."); ;
        return container;
    }

    public static IContainer GetCurrentSceneContainer(this SceneTree sceneTree)
    {
        var rootContainer = sceneTree.GetRootContainer();
        var scene = sceneTree.CurrentScene
            ?? throw new InvalidOperationException("CurrentScene found in SceneTree.");

        var container = rootContainer.FindChild(scene.Name.ToString())
            ?? throw new InvalidOperationException($"Container not found for scene: {scene.Name}");

        return container;
    }
}
