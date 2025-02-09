using GroveGames.DependencyInjection.Collections;
using GroveGames.DependencyInjection.Resolution;

namespace GroveGames.DependencyInjection.Tests.Resolution;

public class UninitializedObjectResolverTests
{
    [Fact]
    public void Resolve_ShouldReturnInstanceOfImplementationType()
    {
        // Arrange
        var mockDisposable = new Mock<IDisposable>();
        var implementationType = mockDisposable.Object.GetType();
        var mockRegistrationResolver = new Mock<IObjectResolver>();
        var mockDisposableCollection = new Mock<IDisposableCollection>();
        var objectResolver = new UninitializedObjectResolver(implementationType, mockRegistrationResolver.Object, mockDisposableCollection.Object);

        // Act
        var result = objectResolver.Resolve();

        // Assert
        Assert.NotNull(result);
        Assert.IsType(implementationType, result);
    }

    [Fact]
    public void Resolve_ShouldInjectDependencies()
    {
        // Arrange
        var mockDisposable = new Mock<IDisposable>();
        var implementationType = mockDisposable.Object.GetType();
        var mockRegistrationResolver = new Mock<IObjectResolver>();
        var mockDisposableCollection = new Mock<IDisposableCollection>();
        var objectResolver = new UninitializedObjectResolver(implementationType, mockRegistrationResolver.Object, mockDisposableCollection.Object);

        // Act
        var result = objectResolver.Resolve();

        // Assert
        mockRegistrationResolver.Verify(r => r.Resolve(It.IsAny<Type>()), Times.AtLeastOnce);
    }

    [Fact]
    public void Resolve_ShouldAddToDisposableCollection()
    {
        // Arrange
        var mockDisposable = new Mock<IDisposable>();
        var implementationType = mockDisposable.Object.GetType();
        var mockRegistrationResolver = new Mock<IObjectResolver>();
        var mockDisposableCollection = new Mock<IDisposableCollection>();
        var objectResolver = new UninitializedObjectResolver(implementationType, mockRegistrationResolver.Object, mockDisposableCollection.Object);

        // Act
        var result = objectResolver.Resolve();

        // Assert
        mockDisposableCollection.Verify(d => d.TryAdd(result), Times.Once);
    }
}