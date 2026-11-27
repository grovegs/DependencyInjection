using Godot;

namespace GroveGames.DependencyInjection.Godot;

[GlobalClass]
public partial class DependencyInjectionSettingsResource : Resource, IDependencyInjectionSettings
{
    private const string ProjectSettingsKey = "grove_games/dependency_injection/settings_resource";
    private const string DefaultResourcePath = "res://addons/GroveGames.DependencyInjection/DependencyInjectionSettings.tres";

    [Export] public RootInstaller RootInstaller { get; set; }
    IInstaller? IDependencyInjectionSettings.RootInstaller => RootInstaller;

    public static DependencyInjectionSettingsResource GetOrCreate()
    {
        if (ProjectSettings.HasSetting(ProjectSettingsKey))
        {
            var resourcePath = ProjectSettings.GetSetting(ProjectSettingsKey).AsString();

            if (ResourceLoader.Exists(resourcePath) && ResourceLoader.Load(resourcePath) is DependencyInjectionSettingsResource configured)
            {
                return configured;
            }
        }

        if (ResourceLoader.Exists(DefaultResourcePath) && ResourceLoader.Load(DefaultResourcePath) is DependencyInjectionSettingsResource settings)
        {
            return settings;
        }

        GD.PushWarning("The dependency injection settings resource was not found or could not be loaded, so the root container has no installer.");
        return new DependencyInjectionSettingsResource();
    }

    internal static string GetProjectSettingsKey() => ProjectSettingsKey;
    internal static string GetDefaultResourcePath() => DefaultResourcePath;
}
