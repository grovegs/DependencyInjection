namespace GroveGames.DependencyInjection;

internal class SceneLoaderException : Exception
{
    public SceneLoaderException()
    {
    }

    public SceneLoaderException(string? message) : base(message)
    {
    }

    public SceneLoaderException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}