using GroveGames.DependencyInjection.Resolution;

namespace GroveGames.DependencyInjection.Tests.Resolution;

public class SingletonResolverTests
{
    [Fact]
    public void Resolve_ShouldReturnSameInstance()
    {
        var expectedInstance = new object();
        var mockObjectResolver = new TestInstanceResolver(expectedInstance);
        var singletonResolver = new SingletonResolver(mockObjectResolver);

        var instance1 = singletonResolver.Resolve();
        var instance2 = singletonResolver.Resolve();

        Assert.Same(expectedInstance, instance1);
        Assert.Same(instance1, instance2);
    }

    [Fact]
    public void Resolve_ShouldCallObjectResolverOnlyOnce()
    {
        var mockObjectResolver = new TestInstanceResolver(new object());
        var singletonResolver = new SingletonResolver(mockObjectResolver);

        singletonResolver.Resolve();
        singletonResolver.Resolve();

        Assert.Equal(1, mockObjectResolver.ResolveCallCount);
    }

    private sealed class TestInstanceResolver : IInstanceResolver
    {
        private readonly object _returnValue;

        public int ResolveCallCount { get; private set; }

        public TestInstanceResolver(object returnValue)
        {
            _returnValue = returnValue;
        }

        public object Resolve()
        {
            ResolveCallCount++;
            return _returnValue;
        }
    }
}
