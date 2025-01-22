namespace GroveGames.DependencyInjection;

public interface IProjectSettings
{
    bool HasSetting(string key);
    void SetSetting<T>(string key, T value);
    T GetSetting<T>(string key);
}