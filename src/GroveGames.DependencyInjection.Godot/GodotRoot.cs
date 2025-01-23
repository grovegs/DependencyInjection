using Godot;

namespace GroveGames.DependencyInjection;

public sealed class GodotRoot
{
    private readonly Window _window;

    public GodotRoot(Window window)
    {
        _window = window;
    }

    public void Run()
    {
        var settings = new GodotProjectSettings();
        var rootInstallerPath = settings.GetSetting<string>(GodotProjectSettingsKey.RootInstaller);
        var rootInstallerResource = ResourceLoader.Load<Resource>(rootInstallerPath);

        if (rootInstallerResource is not IRootInstaller rootInstaller)
        {
            throw new RootInstallerNotFoundException(rootInstallerPath);
        }

        var rootContainer = RootContainer.Create(rootInstaller.Install);
        _window.TreeExiting += rootContainer.Dispose;
        _window.ChildEnteredTree += InstallScene;
        InstallMainScene(_window);
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