#if TOOLS
using Godot;

namespace GroveGames.DependencyInjection;

[Tool]
public partial class Plugin : EditorPlugin
{
    private const string AutoloadName = "ContainerBootstrapper";
    private const string AutoloadPath = "res://addons/GroveGames.DependencyInjection/ContainerBootstrapper.cs";

    public override void _EnterTree()
    {
        var settingsKey = DependencyInjectionSettingsResource.GetProjectSettingsKey();

        if (!ProjectSettings.HasSetting(settingsKey))
        {
            var resourcePath = DependencyInjectionSettingsResource.GetDefaultResourcePath();
            ProjectSettings.SetSetting(settingsKey, resourcePath);
            ProjectSettings.SetInitialValue(settingsKey, resourcePath);
            ProjectSettings.Save();
        }
    }

    public override void _EnablePlugin()
    {
        AddAutoloadSingleton(AutoloadName, AutoloadPath);
    }

    public override void _DisablePlugin()
    {
        RemoveAutoloadSingleton(AutoloadName);
    }
}
#endif
