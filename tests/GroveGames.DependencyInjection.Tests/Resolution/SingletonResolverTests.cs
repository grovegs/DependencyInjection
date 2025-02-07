using GroveGames.DependencyInjection.Resolution;

namespace GroveGames.DependencyInjection.Tests.Resolution;

public class SingletonResolverTests
{
    [Fact]
    public void Resolve_ShouldReturnSameInstance()
    {
        // Arrange
        var mockObjectResolver = new Mock<IInstanceResolver>();
        var expectedInstance = new object();
        mockObjectResolver.Setup(r => r.Resolve()).Returns(expectedInstance);
        var singletonResolver = new SingletonResolver(mockObjectResolver.Object);

        // Act
        var instance1 = singletonResolver.Resolve();
        var instance2 = singletonResolver.Resolve();

        // Assert
        Assert.Same(expectedInstance, instance1);
        Assert.Same(instance1, instance2);
    }

    [Fact]
    public void Resolve_ShouldCallObjectResolverOnlyOnce()
    {
        // Arrange
        var mockObjectResolver = new Mock<IInstanceResolver>();
        mockObjectResolver.Setup(r => r.Resolve()).Returns(new object());
        var singletonResolver = new SingletonResolver(mockObjectResolver.Object);

        // Act
        singletonResolver.Resolve();
        singletonResolver.Resolve();

        // Assert
        mockObjectResolver.Verify(r => r.Resolve(), Times.Once);
    }
}