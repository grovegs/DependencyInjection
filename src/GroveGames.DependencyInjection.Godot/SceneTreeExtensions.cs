using Godot;

namespace GroveGames.DependencyInjection;

public static class SceneTreeExtensions
{
    public static void ChangeScene(this SceneTree sceneTree, string scenePath, double minDuration, Action<Node>? onSceneCreate = null, Action<Node>? onSceneReady = null)
    {
        var sceneSwitcher = new AsyncSceneSwitcher(sceneTree, scenePath, minDuration, onSceneCreate, onSceneReady);
        sceneSwitcher.RequestSceneSwitch();
    }

    public static void ChangeScene(this SceneTree sceneTree, string scenePath, double minDuration, bool createContainer = true, Action<Node>? onSceneReady = null)
    {
        var onSceneCreate = createContainer ? scene =>
        {
            var rootContainer = sceneTree.GetRootContainer();
            SceneContainerFactory.CreateSceneContainer(scene, rootContainer);
        }
        : (Action<Node>?)null;

        var sceneSwitcher = new AsyncSceneSwitcher(sceneTree, scenePath, minDuration, onSceneCreate, onSceneReady);
        sceneSwitcher.RequestSceneSwitch();
    }

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
            ?? throw new InvalidOperationException("No current scene found in SceneTree.");

        var container = rootContainer.FindChild(scene.Name.ToString())
            ?? throw new InvalidOperationException($"Container not found for scene: {scene.Name}");

        return container;
    }
}
