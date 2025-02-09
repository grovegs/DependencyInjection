using Godot;

namespace GroveGames.DependencyInjection;

public static class SceneContainerFactory
{
    public static void CreateSceneContainer(Node scene, IRootContainer rootContainer)
    {
        foreach (var sceneChild in scene.GetChildren())
        {
            if (sceneChild is not ISceneInstaller installer)
            {
                continue;
            }

            var name = scene.Name.ToString();
            var container = ContainerFactory.CreateContainer(name, rootContainer, installer.Install);
            installer.QueueFree();
            scene.TreeExiting += container.Dispose;
            return;
        }
    }
}