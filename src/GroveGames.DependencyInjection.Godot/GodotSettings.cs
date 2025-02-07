namespace GroveGames.DependencyInjection;

public static class GodotSettings
{
    public static readonly GodotSetting<string> RootInstaller = new("grove_games/dependency_injection/root_installer", "res://RootInstaller.tres");
    public static readonly GodotSetting<bool> AutoLoad = new("grove_games/dependency_injection/auto_load", true);

    public static void CreateIfNotExist()
    {
        RootInstaller.CreateIfNotExist();
        AutoLoad.CreateIfNotExist();
    }
}