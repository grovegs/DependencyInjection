namespace GroveGames.DependencyInjection;

public interface IRootContainer : IContainer
{
    IContainer? FindChild(ReadOnlySpan<char> path);
}
