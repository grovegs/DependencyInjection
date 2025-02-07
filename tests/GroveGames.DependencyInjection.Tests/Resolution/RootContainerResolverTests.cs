using GroveGames.DependencyInjection.Resolution;

namespace GroveGames.DependencyInjection.Tests.Resolution;

public class RootContainerResolverTests
{
    [Fact]
    public void Constructor_ShouldInitializeInstanceResolversDictionary()
    {
        // Act
        var resolver = new RootContainerResolver();

        // Assert
        var fieldInfo = typeof(RootContainerResolver)
            .GetField("_resolversByRegistrationTypes", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var dictionary = (Dictionary<Type, IInstanceResolver>)fieldInfo?.GetValue(resolver)!;
        Assert.NotNull(dictionary);
        Assert.Empty(dictionary);
    }

    [Fact]
    public void AddInstanceResolver_ShouldAddResolverToDictionary()
    {
        // Arrange
        var resolver = new RootContainerResolver();
        var mockInstanceResolver = new Mock<IInstanceResolver>();

        // Act
        resolver.AddResolver(typeof(string), mockInstanceResolver.Object);

        // Assert
        var fieldInfo = typeof(RootContainerResolver)
            .GetField("_resolversByRegistrationTypes", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var dictionary = (Dictionary<Type, IInstanceResolver>)fieldInfo?.GetValue(resolver)!;
        Assert.True(dictionary.ContainsKey(typeof(string)));
        Assert.Equal(mockInstanceResolver.Object, dictionary[typeof(string)]);
    }

    [Fact]
    public void Resolve_ShouldReturnResolvedInstance_WhenTypeIsRegistered()
    {
        // Arrange
        var resolver = new RootContainerResolver();
        var mockInstanceResolver = new Mock<IInstanceResolver>();
        var expectedInstance = new object();
        mockInstanceResolver.Setup(r => r.Resolve()).Returns(expectedInstance);

        resolver.AddResolver(typeof(object), mockInstanceResolver.Object);

        // Act
        var result = resolver.Resolve(typeof(object));

        // Assert
        Assert.Equal(expectedInstance, result);
    }

    [Fact]
    public void Resolve_ShouldThrowInvalidOperationException_WhenTypeIsNotRegistered()
    {
        // Arrange
        var resolver = new RootContainerResolver();

        // Act & Assert
        var exception = Assert.Throws<RegistrationNotFoundException>(() => resolver.Resolve(typeof(object)));
        Assert.Equal("No registration found for type System.Object.", exception.Message);
    }

    [Fact]
    public void Clear_ShouldRemoveAllResolvers()
    {
        // Arrange
        var resolver = new RootContainerResolver();
        var mockInstanceResolver = new Mock<IInstanceResolver>();
        resolver.AddResolver(typeof(object), mockInstanceResolver.Object);

        // Act
        resolver.Clear();

        // Assert
        var fieldInfo = typeof(RootContainerResolver)
            .GetField("_resolversByRegistrationTypes", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var dictionary = (Dictionary<Type, IInstanceResolver>)fieldInfo?.GetValue(resolver)!;
        Assert.Empty(dictionary);
    }
}