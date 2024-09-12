namespace DependencyInjection.Caching;

public interface IContainerCache
{
    IContainer? Find(ReadOnlySpan<char> path);
    void Add(IContainer container);
    void Remove(IContainer container);
    void Clear();
}
