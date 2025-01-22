namespace GroveGames.DependencyInjection;

public interface IProjectSettings
{
    bool HasSetting(string key);
    void SetSetting(string key, string value);
    T GetSetting<T>(string key);
}