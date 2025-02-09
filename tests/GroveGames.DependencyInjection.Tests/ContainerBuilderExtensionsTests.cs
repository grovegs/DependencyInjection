namespace GroveGames.DependencyInjection.Tests;

public class ContainerBuilderExtensionsTests
{
    private interface IMockTest
    {
        public void Execute();
    }

    private class MockTest : IMockTest
    {
        public void Execute() { }
    }

    [Fact]
    public void AddSingleton_WithInstance_ShouldAddSingletonWithImplementationType()
    {
        // Arrange
        var instance = new MockTest();
        var mockContainerBuilder = new Mock<IContainerBuilder>();

        // Act
        mockContainerBuilder.Object.AddSingleton(instance);

        // Assert
        mockContainerBuilder.Verify(c => c.AddSingleton(typeof(MockTest), instance), Times.Once);
    }

    [Fact]
    public void AddSingleton_WithType_ShouldAddSingletonWithImplementationType()
    {
        // Arrange
        var type = typeof(MockTest);
        var mockContainerBuilder = new Mock<IContainerBuilder>();

        // Act
        mockContainerBuilder.Object.AddSingleton(type);

        // Assert
        mockContainerBuilder.Verify(c => c.AddSingleton(type, type), Times.Once);
    }

    [Fact]
    public void AddTransient_WithType_ShouldAddTransientWithImplementationType()
    {
        // Arrange
        var type = typeof(MockTest);
        var mockContainerBuilder = new Mock<IContainerBuilder>();

        // Act
        mockContainerBuilder.Object.AddTransient(type);

        // Assert
        mockContainerBuilder.Verify(c => c.AddTransient(type, type), Times.Once);
    }

    [Fact]
    public void AddSingleton_Generic_WithFactory_ShouldAddSingletonUsingFactory()
    {
        // Arrange
        var mockContainerBuilder = new Mock<IContainerBuilder>();
        Func<MockTest> factory = () => new MockTest();

        // Act
        mockContainerBuilder.Object.AddSingleton(factory);

        // Assert
        mockContainerBuilder.Verify(c => c.AddSingleton(typeof(MockTest), factory), Times.Once);
    }

    [Fact]
    public void AddTransient_Generic_WithFactory_ShouldAddTransientUsingFactory()
    {
        // Arrange
        var mockContainerBuilder = new Mock<IContainerBuilder>();
        Func<MockTest> factory = () => new MockTest();

        // Act
        mockContainerBuilder.Object.AddTransient(factory);

        // Assert
        mockContainerBuilder.Verify(c => c.AddTransient(typeof(MockTest), factory), Times.Once);
    }

    [Fact]
    public void AddSingleton_Generic_ShouldAddSingletonWithType()
    {
        // Arrange
        var mockContainerBuilder = new Mock<IContainerBuilder>();

        // Act
        mockContainerBuilder.Object.AddSingleton<IMockTest, MockTest>();

        // Assert
        mockContainerBuilder.Verify(c => c.AddSingleton(typeof(IMockTest), typeof(MockTest)), Times.Once);
    }

    [Fact]
    public void AddTransient_Generic_ShouldAddTransientWithType()
    {
        // Arrange
        var mockContainerBuilder = new Mock<IContainerBuilder>();

        // Act
        mockContainerBuilder.Object.AddTransient<IMockTest, MockTest>();

        // Assert
        mockContainerBuilder.Verify(c => c.AddTransient(typeof(IMockTest), typeof(MockTest)), Times.Once);
    }

    [Fact]
    public void AddSingleton_Generic_WithInstance_ShouldAddSingletonInstance()
    {
        // Arrange
        var instance = new MockTest();
        var mockContainerBuilder = new Mock<IContainerBuilder>();

        // Act
        mockContainerBuilder.Object.AddSingleton<IMockTest>(instance);

        // Assert
        mockContainerBuilder.Verify(c => c.AddSingleton(typeof(IMockTest), instance), Times.Once);
    }
}