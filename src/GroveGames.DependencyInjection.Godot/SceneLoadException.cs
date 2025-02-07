namespace GroveGames.DependencyInjection;

public class SceneLoadException : Exception
{
    public SceneLoadException(string? message) : base(message) { }
}