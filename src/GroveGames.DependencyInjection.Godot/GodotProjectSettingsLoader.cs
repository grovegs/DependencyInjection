namespace GroveGames.DependencyInjection;

public static class GodotProjectSettingsBuilder
{
    public static IProjectSettings Build()
    {
        var projectSettings = new GodotProjectSettings();

        if (!projectSettings.HasSetting(GodotProjectSettingsKey.RootInstaller))
        {
            projectSettings.SetSetting(GodotProjectSettingsKey.RootInstaller, "res://RootInstaller.tres");
        }

        if (!projectSettings.HasSetting(GodotProjectSettingsKey.AutoLoad))
        {
            projectSettings.SetSetting(GodotProjectSettingsKey.AutoLoad, true);
        }

        return new GodotProjectSettings();
    }
}