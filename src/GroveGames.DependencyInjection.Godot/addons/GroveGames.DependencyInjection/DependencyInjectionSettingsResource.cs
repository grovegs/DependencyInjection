using Godot;

namespace GroveGames.DependencyInjection;

[GlobalClass]
public partial class DependencyInjectionSettingsResource : Resource
{
    private const string ProjectSettingsKey = "grove_games/dependency_injection/settings_resource";
    private const string DefaultResourcePath = "res://addons/GroveGames.DependencyInjection/DependencyInjectionSettings.tres";

    [Export] public Godot.Collections.Array<RootInstaller> RootInstallers { get; set; }

    public DependencyInjectionSettingsResource()
    {
        RootInstallers = new Godot.Collections.Array<RootInstaller>();
    }

    public static DependencyInjectionSettingsResource GetOrCreate()
    {
        if (ProjectSettings.HasSetting(ProjectSettingsKey))
        {
            var resourcePath = ProjectSettings.GetSetting(ProjectSettingsKey).AsString();

            if (ResourceLoader.Exists(resourcePath))
            {
                return ResourceLoader.Load<DependencyInjectionSettingsResource>(resourcePath);
            }
        }

        if (ResourceLoader.Exists(DefaultResourcePath))
        {
            return ResourceLoader.Load<DependencyInjectionSettingsResource>(DefaultResourcePath);
        }

        return new DependencyInjectionSettingsResource();
    }

    internal static string GetProjectSettingsKey() => ProjectSettingsKey;
    internal static string GetDefaultResourcePath() => DefaultResourcePath;
}
