using Godot;

namespace GroveGames.DependencyInjection;

public sealed partial class DependencyInjector : Node
{
    public override void _Ready()
    {
        var settings = new Settings(new GodotProjectSettings());
        var rootInstallerPath = settings.GetRootInstallerSetting();
        var rootInstallerResource = ResourceLoader.Load<Resource>(rootInstallerPath);

        if (rootInstallerResource is not IRootInstaller rootInstaller)
        {
            throw new RootInstallerNotFoundException(rootInstallerPath);
        }

        var rootContainer = RootContainer.Create(rootInstaller.Install);
        var tree = GetTree();
        var root = tree.Root;
        root.TreeExiting += rootContainer.Dispose;
        root.ChildEnteredTree += InstallScene;
        InstallMainScene(root);
    }

    private void InstallMainScene(Node root)
    {
        foreach (var scene in root.GetChildren())
        {
            InstallScene(scene);
        }
    }

    private void InstallScene(Node scene)
    {
        foreach (var sceneNode in scene.GetChildren())
        {
            if (sceneNode is not ISceneInstaller sceneInstaller)
            {
                continue;
            }

            var name = scene.Name.ToString();
            var container = Container.Create(name, sceneInstaller.Install);
            sceneInstaller.QueueFree();
            scene.TreeExiting += container.Dispose;
            return;
        }
    }
}
