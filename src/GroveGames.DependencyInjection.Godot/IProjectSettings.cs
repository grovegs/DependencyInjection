using Godot;

namespace GroveGames.DependencyInjection;

public interface IProjectSettings
{
    bool HasSetting(string path);
    void SetSetting(string path, string value);
    Variant GetSetting(string path);
}
