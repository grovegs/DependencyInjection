namespace GroveGames.DependencyInjection;

public sealed class Settings
{
    private const string RootInstallerPath = "grove_games/dependency_injection/root_installer";
    private const string DefaultRootInstallerPath = "res://RootInstaller.tscn";

    private readonly IProjectSettings _projectSettings;

    public Settings(IProjectSettings projectSettings)
    {
        _projectSettings = projectSettings;
    }

    public void CreateRootInstallerSetting()
    {
        if (!_projectSettings.HasSetting(RootInstallerPath))
        {
            _projectSettings.SetSetting(RootInstallerPath, DefaultRootInstallerPath);
        }
    }

    public string GetRootInstallerSetting()
    {
        return _projectSettings.GetSetting(RootInstallerPath).AsString();
    }
}
