namespace GroveGames.DependencyInjection;

public interface IInstallerNode : IInstaller
{
    public ReadOnlySpan<char> Path { get; }
    void QueueFree();
}
