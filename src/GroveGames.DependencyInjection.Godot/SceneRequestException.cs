using Godot;

namespace GroveGames.DependencyInjection;

public class SceneRequestException : Exception
{
    public SceneRequestException(string? message, Error error) : base(message)
    {
        Error = error;
    }
    public Error Error { get; private set; }
}