namespace GroveGames.DependencyInjection.Tests;

public class ContainerConfigurerExtensionsTests
{
    private class MockInitializable : IInitializable
    {
        public void Initialize() { }
    }

    [Fact]
    public void AddSingleton_ShouldAddSingletonWithType()
    {
        // Arrange
        var type = typeof(object);
        var mockContainerConfigurer = new Mock<IContainerConfigurer>();

        // Act
        mockContainerConfigurer.Object.AddSingleton(type);

        // Assert
        mockContainerConfigurer.Verify(c => c.AddSingleton(type, type), Times.Once);
    }

    [Fact]
    public void AddTransient_ShouldAddTransientWithType()
    {
        // Arrange
        var type = typeof(object);
        var mockContainerConfigurer = new Mock<IContainerConfigurer>();

        // Act
        mockContainerConfigurer.Object.AddTransient(type);

        // Assert
        mockContainerConfigurer.Verify(c => c.AddTransient(type, type), Times.Once);
    }

    [Fact]
    public void AddSingleton_Generic_ShouldAddSingletonWithType()
    {
        // Arrange
        var mockContainerConfigurer = new Mock<IContainerConfigurer>();

        // Act
        mockContainerConfigurer.Object.AddSingleton<IInitializable, MockInitializable>();

        // Assert
        mockContainerConfigurer.Verify(c => c.AddSingleton(typeof(IInitializable), typeof(MockInitializable)), Times.Once);
    }

    [Fact]
    public void AddTransient_Generic_ShouldAddTransientWithType()
    {
        // Arrange
        var mockContainerConfigurer = new Mock<IContainerConfigurer>();

        // Act
        mockContainerConfigurer.Object.AddTransient<IInitializable, MockInitializable>();

        // Assert
        mockContainerConfigurer.Verify(c => c.AddTransient(typeof(IInitializable), typeof(MockInitializable)), Times.Once);
    }

    [Fact]
    public void AddSingleton_WithInstance_ShouldAddSingletonInstance()
    {
        // Arrange
        var instance = new MockInitializable();
        var mockContainerConfigurer = new Mock<IContainerConfigurer>();

        // Act
        mockContainerConfigurer.Object.AddSingleton(instance);

        // Assert
        mockContainerConfigurer.Verify(c => c.AddSingleton(typeof(MockInitializable), instance), Times.Once);
    }

    [Fact]
    public void AddSingleton_Generic_WithInstance_ShouldAddSingletonInstance()
    {
        // Arrange
        var instance = new MockInitializable();
        var mockContainerConfigurer = new Mock<IContainerConfigurer>();

        // Act
        mockContainerConfigurer.Object.AddSingleton<IInitializable>(instance);

        // Assert
        mockContainerConfigurer.Verify(c => c.AddSingleton(typeof(IInitializable), instance), Times.Once);
    }
}