using Godot;

namespace GroveGames.DependencyInjection;

public static class SceneTreeExtensions
{
    public static void ChangeScene(this SceneTree sceneTree, string scenePath, double minDuration, Action? onLoaded = null)
    {
        var sceneLoader = new SceneLoader(sceneTree, scenePath, minDuration, onLoaded);
        sceneLoader.LoadRequest();
    }
}
