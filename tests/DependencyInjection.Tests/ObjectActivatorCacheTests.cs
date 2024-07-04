using DependencyInjection.Activators;
using DependencyInjection.Tests.Fakes;
using DependencyInjection.Tests.Fixtures;

namespace DependencyInjection.Tests;

public sealed class ObjectActivatorCacheTests : IClassFixture<ObjectActivatorCacheFixture>
{
    private readonly ObjectActivatorCacheFixture _objectActivatorCacheFixture;

    public ObjectActivatorCacheTests(ObjectActivatorCacheFixture objectActivatorCacheFixture)
    {
        _objectActivatorCacheFixture = objectActivatorCacheFixture;
    }

    [Fact]
    public void TryGet_NotCachedItem_ShouldReturnFalse()
    {
        using var fixture = _objectActivatorCacheFixture;
        var actual = ObjectActivatorCache.TryGet(typeof(ZeroParameterClass), out var objectActivator);
        var expected = false;
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void TryGet_CachedItem_ShouldReturnObjectActivator()
    {
        using var fixture = _objectActivatorCacheFixture;
        var objectActivator = new Mock<IObjectActivator>();
        ObjectActivatorCache.Add(typeof(ZeroParameterClass), objectActivator.Object);
        ObjectActivatorCache.TryGet(typeof(ZeroParameterClass), out var cachedObjectActivator);
        Assert.Same(objectActivator.Object, cachedObjectActivator);
    }

    [Fact]
    public void TryGet_MultipleTimesWithSameImplementationType_ShouldReturnSameObjectActivator()
    {
        using var fixture = _objectActivatorCacheFixture;
        var objectActivator = new Mock<IObjectActivator>();
        ObjectActivatorCache.Add(typeof(ZeroParameterClass), objectActivator.Object);
        ObjectActivatorCache.TryGet(typeof(ZeroParameterClass), out var activator1);
        ObjectActivatorCache.TryGet(typeof(ZeroParameterClass), out var activator2);
        Assert.Same(activator1, activator2);
    }
}
