using DependencyInjection.Activators;

namespace DependencyInjection.Tests.Fixtures;

public sealed class ObjectActivatorCacheFixture : IDisposable
{
    public ObjectActivatorCacheFixture()
    {
        ObjectActivatorCache.Clear();
    }

    public void Dispose()
    {
        ObjectActivatorCache.Clear();
    }
}
