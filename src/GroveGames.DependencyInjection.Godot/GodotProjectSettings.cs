using Godot;

namespace GroveGames.DependencyInjection;

public sealed class GodotProjectSettings : IProjectSettings
{
    public Variant GetSetting(string path)
    {
        return ProjectSettings.GetSetting(path);
    }

    public bool HasSetting(string path)
    {
        return ProjectSettings.HasSetting(path);
    }

    public void SetSetting(string path, string value)
    {
        ProjectSettings.SetSetting(path, value);
    }
}
