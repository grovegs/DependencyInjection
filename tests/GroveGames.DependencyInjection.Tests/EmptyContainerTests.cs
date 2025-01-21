namespace GroveGames.DependencyInjection.Tests;

public class EmptyContainerTests
{
    [Fact]
    public void Name_ShouldReturnEmptyString()
    {
        // Arrange
        var emptyContainer = new EmptyContainer();

        // Act
        var name = emptyContainer.Name;

        // Assert
        Assert.Equal(string.Empty, name);
    }

    [Fact]
    public void Parent_ShouldReturnNull()
    {
        // Arrange
        var emptyContainer = new EmptyContainer();

        // Act
        var parent = emptyContainer.Parent;

        // Assert
        Assert.Null(parent);
    }

    [Fact]
    public void AddChild_ShouldDoNothing()
    {
        // Arrange
        var emptyContainer = new EmptyContainer();
        var mockChild = new Mock<IContainer>();

        // Act
        emptyContainer.AddChild(mockChild.Object);

        // Assert
    }

    [Fact]
    public void RemoveChild_ShouldDoNothing()
    {
        // Arrange
        var emptyContainer = new EmptyContainer();
        var mockChild = new Mock<IContainer>();

        // Act
        emptyContainer.RemoveChild(mockChild.Object);

        // Assert
    }

    [Fact]
    public void Dispose_ShouldDoNothing()
    {
        // Arrange
        var emptyContainer = new EmptyContainer();

        // Act
        emptyContainer.Dispose();

        // Assert
    }

    [Fact]
    public void Resolve_ShouldReturnNull()
    {
        // Arrange
        var emptyContainer = new EmptyContainer();
        var registrationType = typeof(object);

        // Act
        var result = emptyContainer.Resolve(registrationType);

        // Assert
        Assert.Null(result);
    }
}