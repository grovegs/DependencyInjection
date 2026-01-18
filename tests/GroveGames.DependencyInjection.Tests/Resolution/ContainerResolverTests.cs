using GroveGames.DependencyInjection.Resolution;

namespace GroveGames.DependencyInjection.Tests.Resolution;

public class ContainerResolverTests
{
    [Fact]
    public void Resolve_ShouldReturnInstanceFromInstanceResolver()
    {
        var registrationType = typeof(object);
        var expectedInstance = new object();
        var mockInstanceResolver = new TestInstanceResolver(expectedInstance);
        var mockParentResolver = new TestObjectResolver();
        var containerResolver = new ContainerResolver(mockParentResolver);
        containerResolver.AddResolver(registrationType, mockInstanceResolver);

        var resolvedInstance = containerResolver.Resolve(registrationType);

        Assert.Equal(expectedInstance, resolvedInstance);
    }

    [Fact]
    public void Resolve_ShouldReturnInstanceFromParentResolver_WhenInstanceResolverNotFound()
    {
        var registrationType = typeof(object);
        var expectedInstance = new object();
        var mockParentResolver = new TestObjectResolver();
        mockParentResolver.SetupResolve(registrationType, expectedInstance);
        var containerResolver = new ContainerResolver(mockParentResolver);

        var resolvedInstance = containerResolver.Resolve(registrationType);

        Assert.Equal(expectedInstance, resolvedInstance);
    }

    [Fact]
    public void AddInstanceResolver_ShouldAddInstanceResolver()
    {
        var registrationType = typeof(object);
        var mockInstanceResolver = new TestInstanceResolver(new object());
        var mockParentResolver = new TestObjectResolver();
        var containerResolver = new ContainerResolver(mockParentResolver);

        containerResolver.AddResolver(registrationType, mockInstanceResolver);

        var field = typeof(ContainerResolver).GetField("_resolversByRegistrationTypes", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        Assert.NotNull(field);
        var instanceResolvers = field.GetValue(containerResolver) as Dictionary<Type, IInstanceResolver>;
        Assert.NotNull(instanceResolvers);
        Assert.True(instanceResolvers.ContainsKey(registrationType));
    }

    [Fact]
    public void Clear_ShouldRemoveAllInstanceResolvers()
    {
        var registrationType = typeof(object);
        var mockInstanceResolver = new TestInstanceResolver(new object());
        var mockParentResolver = new TestObjectResolver();
        var containerResolver = new ContainerResolver(mockParentResolver);
        containerResolver.AddResolver(registrationType, mockInstanceResolver);

        containerResolver.Clear();

        var field = typeof(ContainerResolver).GetField("_resolversByRegistrationTypes", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        Assert.NotNull(field);
        var instanceResolvers = field.GetValue(containerResolver) as Dictionary<Type, IInstanceResolver>;
        Assert.NotNull(instanceResolvers);
        Assert.Empty(instanceResolvers);
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

    private sealed class TestObjectResolver : IObjectResolver
    {
        private readonly Dictionary<Type, object> _returnValues = new();

        public void SetupResolve(Type type, object returnValue)
        {
            _returnValues[type] = returnValue;
        }

        public object Resolve(Type registrationType)
        {
            return _returnValues.TryGetValue(registrationType, out var value) ? value : null!;
        }
    }
}
