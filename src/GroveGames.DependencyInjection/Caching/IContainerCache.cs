namespace GroveGames.DependencyInjection.Caching;

public interface IContainerCache
{
    IContainer? Find(in ReadOnlySpan<char> path);
    void Add(IContainer container);
    void Remove(IContainer container);
    void Clear();
}
