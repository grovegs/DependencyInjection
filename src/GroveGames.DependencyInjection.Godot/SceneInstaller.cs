using Godot;

namespace GroveGames.DependencyInjection;

public static class SceneInstaller
{
    public static void Install(Node scene, IContainer parent)
    {
        foreach (var sceneChild in scene.GetChildren())
        {
            if (sceneChild is not ISceneInstaller sceneInstaller)
            {
                continue;
            }

            var name = scene.Name.ToString();
            var container = ContainerFactory.CreateContainer(name, parent, sceneInstaller.Install);
            sceneInstaller.QueueFree();
            scene.TreeExiting += container.Dispose;
            return;
        }
    }

    public static void Install(Node scene, Window window)
    {
        var parent = window.GetContainer();
        Install(scene, parent);
    }
}