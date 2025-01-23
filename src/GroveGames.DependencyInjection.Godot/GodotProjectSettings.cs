using Godot;

namespace GroveGames.DependencyInjection;

public sealed class GodotProjectSettings : IProjectSettings
{
    public T GetSetting<[MustBeVariant] T>(string key)
    {
        return ProjectSettings.GetSetting(key).As<T>();
    }

    public bool HasSetting(string key)
    {
        return ProjectSettings.HasSetting(key);
    }

    public void SetSetting<[MustBeVariant] T>(string key, T value)
    {
        ProjectSettings.SetSetting(key, Variant.From(value));
    }
}
