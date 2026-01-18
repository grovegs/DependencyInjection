using GroveGames.DependencyInjection.Resolution;

namespace GroveGames.DependencyInjection.Tests.Resolution;

public class RootContainerResolverTests
{
    [Fact]
    public void Constructor_ShouldInitializeInstanceResolversDictionary()
    {
        var resolver = new RootContainerResolver();

        var fieldInfo = typeof(RootContainerResolver)
            .GetField("_resolversByRegistrationTypes", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var dictionary = (Dictionary<Type, IInstanceResolver>)fieldInfo?.GetValue(resolver)!;
        Assert.NotNull(dictionary);
        Assert.Empty(dictionary);
    }

    [Fact]
    public void AddInstanceResolver_ShouldAddResolverToDictionary()
    {
        var resolver = new RootContainerResolver();
        var mockInstanceResolver = new TestInstanceResolver(new object());

        resolver.AddResolver(typeof(string), mockInstanceResolver);

        var fieldInfo = typeof(RootContainerResolver)
            .GetField("_resolversByRegistrationTypes", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var dictionary = (Dictionary<Type, IInstanceResolver>)fieldInfo?.GetValue(resolver)!;
        Assert.True(dictionary.ContainsKey(typeof(string)));
        Assert.Equal(mockInstanceResolver, dictionary[typeof(string)]);
    }

    [Fact]
    public void Resolve_ShouldReturnResolvedInstance_WhenTypeIsRegistered()
    {
        var resolver = new RootContainerResolver();
        var expectedInstance = new object();
        var mockInstanceResolver = new TestInstanceResolver(expectedInstance);

        resolver.AddResolver(typeof(object), mockInstanceResolver);

        var result = resolver.Resolve(typeof(object));

        Assert.Equal(expectedInstance, result);
    }

    [Fact]
    public void Resolve_ShouldThrowInvalidOperationException_WhenTypeIsNotRegistered()
    {
        var resolver = new RootContainerResolver();

        var exception = Assert.Throws<RegistrationNotFoundException>(() => resolver.Resolve(typeof(object)));
        Assert.Equal("No registration found for type System.Object.", exception.Message);
    }

    [Fact]
    public void Clear_ShouldRemoveAllResolvers()
    {
        var resolver = new RootContainerResolver();
        var mockInstanceResolver = new TestInstanceResolver(new object());
        resolver.AddResolver(typeof(object), mockInstanceResolver);

        resolver.Clear();

        var fieldInfo = typeof(RootContainerResolver)
            .GetField("_resolversByRegistrationTypes", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var dictionary = (Dictionary<Type, IInstanceResolver>)fieldInfo?.GetValue(resolver)!;
        Assert.Empty(dictionary);
    }

    private sealed class TestInstanceResolver : IInstanceResolver
    {
        private readonly object _returnValue;

        public TestInstanceResolver(object returnValue)
        {
            _returnValue = returnValue;
        }

        public object Resolve()
        {
            return _returnValue;
        }
    }
}
