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
        SceneInstaller.Install(_window);
    }
}
