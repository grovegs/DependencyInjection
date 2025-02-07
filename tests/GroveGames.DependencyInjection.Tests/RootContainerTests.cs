namespace GroveGames.DependencyInjection.Tests;

public class RootContainerTests
{
    [Fact]
    public void Constructor_ShouldInitializeRootContainer()
    {
        // Arrange
        var mockContainer = new Mock<IContainer>();

        // Act
        var rootContainer = new RootContainer(mockContainer.Object);

        // Assert
        Assert.NotNull(rootContainer);
        Assert.Equal(mockContainer.Object.Name, rootContainer.Name);
        Assert.Equal(mockContainer.Object.Parent, rootContainer.Parent);
        Assert.Equal(mockContainer.Object.Cache, rootContainer.Cache);
    }

    [Fact]
    public void AddChild_ShouldDelegateToInternalContainer()
    {
        // Arrange
        var mockContainer = new Mock<IContainer>();
        var mockChild = new Mock<IContainer>();
        var rootContainer = new RootContainer(mockContainer.Object);

        // Act
        rootContainer.AddChild(mockChild.Object);

        // Assert
        mockContainer.Verify(c => c.AddChild(mockChild.Object), Times.Once);
    }

    [Fact]
    public void RemoveChild_ShouldDelegateToInternalContainer()
    {
        // Arrange
        var mockContainer = new Mock<IContainer>();
        var mockChild = new Mock<IContainer>();
        var rootContainer = new RootContainer(mockContainer.Object);

        // Act
        rootContainer.RemoveChild(mockChild.Object);

        // Assert
        mockContainer.Verify(c => c.RemoveChild(mockChild.Object), Times.Once);
    }

    [Fact]
    public void Resolve_ShouldDelegateToInternalContainer()
    {
        // Arrange
        var mockContainer = new Mock<IContainer>();
        var mockObject = new object();
        mockContainer.Setup(c => c.Resolve(typeof(object))).Returns(mockObject);
        var rootContainer = new RootContainer(mockContainer.Object);

        // Act
        var result = rootContainer.Resolve(typeof(object));

        // Assert
        Assert.Equal(mockObject, result);
        mockContainer.Verify(c => c.Resolve(typeof(object)), Times.Once);
    }

    [Fact]
    public void Dispose_ShouldDelegateToInternalContainer()
    {
        // Arrange
        var mockContainer = new Mock<IContainer>();
        var rootContainer = new RootContainer(mockContainer.Object);

        // Act
        rootContainer.Dispose();

        // Assert
        mockContainer.Verify(c => c.Dispose(), Times.Once);
    }
}