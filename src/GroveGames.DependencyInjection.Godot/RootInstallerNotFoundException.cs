namespace GroveGames.DependencyInjection;

public class RootInstallerNotFoundException : Exception
{
    public RootInstallerNotFoundException(string path) : base($"Root installer not found at path: {path}") { }
}