using GroveGames.DependencyInjection.Collections;
using GroveGames.DependencyInjection.Resolution;

namespace GroveGames.DependencyInjection.Tests.Collections;

public class InitializableCollectionTests
{
    private class MockInitializable : IInitializable
    {
        public void Initialize()
        {
        }
    }

    [Fact]
    public void TryAdd_ShouldAddInitializableType()
    {
        // Arrange
        var registrationType = typeof(IInitializable);
        var implementationType = typeof(MockInitializable);
        var mockRegistrationResolver = new Mock<IRegistrationResolver>();
        var initializableCollection = new InitializableCollection(mockRegistrationResolver.Object);

        // Act
        initializableCollection.TryAdd(registrationType, implementationType);

        // Assert
        var field = typeof(InitializableCollection).GetField("_initializableRegistrationTypes", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        Assert.NotNull(field);
        var initializableRegistrationTypes = field.GetValue(initializableCollection) as List<Type>;
        Assert.NotNull(initializableRegistrationTypes);
        Assert.Contains(registrationType, initializableRegistrationTypes);
    }

    [Fact]
    public void TryAdd_ShouldNotAddNonInitializableType()
    {
        // Arrange
        var registrationType = typeof(object);
        var implementationType = typeof(object);
        var mockRegistrationResolver = new Mock<IRegistrationResolver>();
        var initializableCollection = new InitializableCollection(mockRegistrationResolver.Object);

        // Act
        initializableCollection.TryAdd(registrationType, implementationType);

        // Assert
        var field = typeof(InitializableCollection).GetField("_initializableRegistrationTypes", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        Assert.NotNull(field);
        var initializableRegistrationTypes = field.GetValue(initializableCollection) as List<Type>;
        Assert.NotNull(initializableRegistrationTypes);
        Assert.DoesNotContain(registrationType, initializableRegistrationTypes);
    }

    [Fact]
    public void Initialize_ShouldCallInitializeOnAllInitializables()
    {
        // Arrange
        var registrationType = typeof(MockInitializable);
        var mockInitializable = new Mock<IInitializable>();
        var mockRegistrationResolver = new Mock<IRegistrationResolver>();
        var initializableCollection = new InitializableCollection(mockRegistrationResolver.Object);
        mockRegistrationResolver.Setup(r => r.Resolve(registrationType)).Returns(mockInitializable.Object);
        initializableCollection.TryAdd(registrationType, registrationType);

        // Act
        initializableCollection.Initialize();

        // Assert
        mockInitializable.Verify(i => i.Initialize(), Times.Once);
    }
}
