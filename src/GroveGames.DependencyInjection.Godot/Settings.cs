namespace GroveGames.DependencyInjection;

public sealed class Settings
{
    private const string RootInstallerPathKey = "grove_games/dependency_injection/root_installer";
    private const string DefaultRootInstallerPath = "res://RootInstaller.tres";

    private readonly IProjectSettings _projectSettings;

    public Settings(IProjectSettings projectSettings)
    {
        _projectSettings = projectSettings;
    }

    public void CreateRootInstallerSetting()
    {
        if (!_projectSettings.HasSetting(RootInstallerPathKey))
        {
            _projectSettings.SetSetting(RootInstallerPathKey, DefaultRootInstallerPath);
        }
    }

    public string GetRootInstallerSetting()
    {
        return _projectSettings.GetSetting<string>(RootInstallerPathKey);
    }
}
