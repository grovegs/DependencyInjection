using GroveGames.DependencyInjection.Resolution;

namespace GroveGames.DependencyInjection.Tests.Resolution;

public class TransientResolverTests
{
    [Fact]
    public void Resolve_ShouldCallObjectResolverResolve()
    {
        var expectedObject = new object();
        var mockObjectResolver = new TestInstanceResolver(expectedObject);
        var transientResolver = new TransientResolver(mockObjectResolver);

        var resolvedObject = transientResolver.Resolve();

        Assert.Equal(1, mockObjectResolver.ResolveCallCount);
        Assert.Equal(expectedObject, resolvedObject);
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
