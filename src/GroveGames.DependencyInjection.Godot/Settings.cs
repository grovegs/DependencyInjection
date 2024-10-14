using Godot;

namespace GroveGames.DependencyInjection;

public sealed class Settings
{
    private const string RootInstallerPath = "grove_games/dependency_injection/root_installer";
    private const string DefaultRootInstallerPath = "res://RootInstaller.tscn";

    public static void CreateRootInstallerSetting()
    {
        if (!ProjectSettings.HasSetting(RootInstallerPath))
        {
            ProjectSettings.SetSetting(RootInstallerPath, DefaultRootInstallerPath);
        }
    }

    public static string GetRootInstallerSetting()
    {
        return ProjectSettings.GetSetting(RootInstallerPath).AsString();
    }
}
