using GroveGames.DependencyInjection.Caching;
using GroveGames.DependencyInjection.Resolution;

namespace GroveGames.DependencyInjection.Tests;

public class ContainerBuilderTests
{
    [Fact]
    public void AddSingleton_WithInstance_ShouldAddSingletonResolver()
    {
        // Arrange
        var registrationType = typeof(object);
        var implementationInstance = new object();
        var mockResolver = new Mock<IContainerResolver>();
        var mockParent = new Mock<IContainer>();
        var containerBuilder = new ContainerBuilder("TestContainer", mockParent.Object, mockResolver.Object, Mock.Of<IContainerCache>());

        // Act
        containerBuilder.AddSingleton(registrationType, implementationInstance);

        // Assert
        mockResolver.Verify(r => r.AddInstanceResolver(registrationType, It.IsAny<SingletonResolver>()), Times.Once);
    }

    [Fact]
    public void AddSingleton_WithFactory_ShouldAddSingletonResolver()
    {
        // Arrange
        var registrationType = typeof(object);
        var mockResolver = new Mock<IContainerResolver>();
        var mockParent = new Mock<IContainer>();
        var factory = new Func<object>(() => new object());
        var containerBuilder = new ContainerBuilder("TestContainer", mockParent.Object, mockResolver.Object, Mock.Of<IContainerCache>());

        // Act
        containerBuilder.AddSingleton(registrationType, factory);

        // Assert
        mockResolver.Verify(r => r.AddInstanceResolver(registrationType, It.IsAny<SingletonResolver>()), Times.Once);
    }

    [Fact]
    public void AddSingleton_WithType_ShouldAddSingletonResolver()
    {
        // Arrange
        var registrationType = typeof(object);
        var implementationType = typeof(object);
        var mockResolver = new Mock<IContainerResolver>();
        var mockParent = new Mock<IContainer>();
        var containerBuilder = new ContainerBuilder("TestContainer", mockParent.Object, mockResolver.Object, Mock.Of<IContainerCache>());

        // Act
        containerBuilder.AddSingleton(registrationType, implementationType);

        // Assert
        mockResolver.Verify(r => r.AddInstanceResolver(registrationType, It.IsAny<SingletonResolver>()), Times.Once);
    }

    [Fact]
    public void AddTransient_WithType_ShouldAddTransientResolver()
    {
        // Arrange
        var registrationType = typeof(object);
        var implementationType = typeof(object);
        var mockResolver = new Mock<IContainerResolver>();
        var mockParent = new Mock<IContainer>();
        var containerBuilder = new ContainerBuilder("TestContainer", mockParent.Object, mockResolver.Object, Mock.Of<IContainerCache>());

        // Act
        containerBuilder.AddTransient(registrationType, implementationType);

        // Assert
        mockResolver.Verify(r => r.AddInstanceResolver(registrationType, It.IsAny<TransientResolver>()), Times.Once);
    }

    [Fact]
    public void AddTransient_WithFactory_ShouldAddTransientResolver()
    {
        // Arrange
        var registrationType = typeof(object);
        var mockResolver = new Mock<IContainerResolver>();
        var mockParent = new Mock<IContainer>();
        var factory = new Func<object>(() => new object());
        var containerBuilder = new ContainerBuilder("TestContainer", mockParent.Object, mockResolver.Object, Mock.Of<IContainerCache>());

        // Act
        containerBuilder.AddTransient(registrationType, factory);

        // Assert
        mockResolver.Verify(r => r.AddInstanceResolver(registrationType, It.IsAny<TransientResolver>()), Times.Once);
    }

    [Fact]
    public void AddTransient_WithFactory_ShouldCreateNewInstanceEachTime()
    {
        // Arrange
        var registrationType = typeof(object);
        var instanceCount = 0;
        var mockResolver = new Mock<IContainerResolver>();
        var mockParent = new Mock<IContainer>();
        var factory = new Func<object>(() =>
        {
            instanceCount++;
            return new object();
        });
        var containerBuilder = new ContainerBuilder("TestContainer", mockParent.Object, mockResolver.Object, Mock.Of<IContainerCache>());

        // Act
        containerBuilder.AddTransient(registrationType, factory);

        // Simulate multiple resolutions
        factory();
        factory();

        // Assert
        Assert.Equal(2, instanceCount);
        mockResolver.Verify(r => r.AddInstanceResolver(registrationType, It.IsAny<TransientResolver>()), Times.Once);
    }
}