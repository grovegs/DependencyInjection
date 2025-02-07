using Godot;

namespace GroveGames.DependencyInjection;

public static class SceneTreeExtensions
{
    public static void ChangeScene(this SceneTree sceneTree, string scenePath, double minDuration, Action? onSceneReady = null)
    {
        var sceneLoader = new AsyncSceneSwitcher(sceneTree, scenePath, minDuration, onSceneReady);
        sceneLoader.RequestSceneSwitch();
    }

    public static IContainer GetCurrentSceneContainer(this SceneTree sceneTree)
    {
        var rootContainer = sceneTree.Root.GetContainer();
        var scene = sceneTree.CurrentScene
            ?? throw new InvalidOperationException("No current scene found in SceneTree.");

        var container = rootContainer.FindChild(scene.Name.ToString())
            ?? throw new InvalidOperationException($"Container not found for scene: {scene.Name}");

        return container;
    }
}
