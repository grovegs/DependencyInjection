using GroveGames.DependencyInjection.Collections;
using GroveGames.DependencyInjection.Resolution;

namespace GroveGames.DependencyInjection.Tests;

public class ContainerConfigurerTests
{
    [Fact]
    public void AddSingleton_WithInstance_ShouldAddSingletonResolverWithInitializedObjectResolver()
    {
        // Arrange
        var registrationType = typeof(object);
        var implementationInstance = new object();
        var mockContainerResolver = new Mock<IContainerResolver>();
        var mockInitializableCollection = new Mock<IInitializableCollection>();
        var mockDisposableCollection = new Mock<IDisposableCollection>();
        var containerConfigurer = new ContainerConfigurer(mockContainerResolver.Object, mockInitializableCollection.Object, mockDisposableCollection.Object);

        // Act
        containerConfigurer.AddSingleton(registrationType, implementationInstance);

        // Assert
        mockContainerResolver.Verify(r => r.AddInstanceResolver(registrationType, It.IsAny<SingletonResolver>()), Times.Once);
        mockInitializableCollection.Verify(i => i.TryAdd(registrationType, registrationType), Times.Once);
    }

    [Fact]
    public void AddSingleton_WithFactory_ShouldAddSingletonResolverWithFactoryObjectResolver()
    {
        // Arrange
        var registrationType = typeof(object);
        var mockContainerResolver = new Mock<IContainerResolver>();
        var mockInitializableCollection = new Mock<IInitializableCollection>();
        var mockDisposableCollection = new Mock<IDisposableCollection>();
        var factory = new Func<object>(() => new object());
        var containerConfigurer = new ContainerConfigurer(mockContainerResolver.Object, mockInitializableCollection.Object, mockDisposableCollection.Object);

        // Act
        containerConfigurer.AddSingleton(registrationType, factory);

        // Assert
        mockContainerResolver.Verify(r => r.AddInstanceResolver(registrationType, It.IsAny<SingletonResolver>()), Times.Once);
        mockInitializableCollection.Verify(i => i.TryAdd(registrationType, registrationType), Times.Once);
    }

    [Fact]
    public void AddSingleton_WithoutInstance_ShouldCreateObjectResolverAndAddSingletonResolver()
    {
        // Arrange
        var registrationType = typeof(object);
        var implementationType = typeof(object);
        var mockContainerResolver = new Mock<IContainerResolver>();
        var mockInitializableCollection = new Mock<IInitializableCollection>();
        var mockDisposableCollection = new Mock<IDisposableCollection>();
        var containerConfigurer = new ContainerConfigurer(mockContainerResolver.Object, mockInitializableCollection.Object, mockDisposableCollection.Object);

        // Act
        containerConfigurer.AddSingleton(registrationType, implementationType);

        // Assert
        mockContainerResolver.Verify(r => r.AddInstanceResolver(registrationType, It.IsAny<SingletonResolver>()), Times.Once);
        mockInitializableCollection.Verify(i => i.TryAdd(registrationType, implementationType), Times.Once);
    }

    [Fact]
    public void AddTransient_ShouldAddTransientResolverAndTryAddToInitializableCollection()
    {
        // Arrange
        var registrationType = typeof(object);
        var implementationType = typeof(object);
        var mockContainerResolver = new Mock<IContainerResolver>();
        var mockInitializableCollection = new Mock<IInitializableCollection>();
        var mockDisposableCollection = new Mock<IDisposableCollection>();
        var containerConfigurer = new ContainerConfigurer(mockContainerResolver.Object, mockInitializableCollection.Object, mockDisposableCollection.Object);

        // Act
        containerConfigurer.AddTransient(registrationType, implementationType);

        // Assert
        mockContainerResolver.Verify(r => r.AddInstanceResolver(registrationType, It.IsAny<TransientResolver>()), Times.Once);
        mockInitializableCollection.Verify(i => i.TryAdd(registrationType, implementationType), Times.Once);
    }

    [Fact]
    public void AddSingleton_WithFactory_ShouldCreateInstanceWhenResolved()
    {
        // Arrange
        var registrationType = typeof(object);
        var mockContainerResolver = new Mock<IContainerResolver>();
        var mockInitializableCollection = new Mock<IInitializableCollection>();
        var mockDisposableCollection = new Mock<IDisposableCollection>();
        var factoryInvoked = false;
        var factory = new Func<object>(() =>
        {
            factoryInvoked = true;
            return new object();
        });

        var containerConfigurer = new ContainerConfigurer(mockContainerResolver.Object, mockInitializableCollection.Object, mockDisposableCollection.Object);

        // Act
        containerConfigurer.AddSingleton(registrationType, factory);

        // Assert
        Assert.False(factoryInvoked, "Factory should not be invoked during registration");
        mockContainerResolver.Verify(r => r.AddInstanceResolver(registrationType, It.IsAny<SingletonResolver>()), Times.Once);
    }
}