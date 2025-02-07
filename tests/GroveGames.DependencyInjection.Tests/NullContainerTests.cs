namespace GroveGames.DependencyInjection.Tests;

public class NullContainerTests
{
    [Fact]
    public void Name_ShouldReturnEmptyString()
    {
        // Arrange
        var container = new NullContainer();

        // Act
        var name = container.Name;

        // Assert
        Assert.Equal(string.Empty, name);
    }

    [Fact]
    public void Parent_ShouldReturnNull()
    {
        // Arrange
        var container = new NullContainer();

        // Act
        var parent = container.Parent;

        // Assert
        Assert.Null(parent);
    }

    [Fact]
    public void AddChild_ShouldDoNothing()
    {
        // Arrange
        var container = new NullContainer();
        var mockChild = new Mock<IContainer>();

        // Act
        container.AddChild(mockChild.Object);

        // Assert
    }

    [Fact]
    public void RemoveChild_ShouldDoNothing()
    {
        // Arrange
        var container = new NullContainer();
        var mockChild = new Mock<IContainer>();

        // Act
        container.RemoveChild(mockChild.Object);

        // Assert
    }

    [Fact]
    public void Dispose_ShouldDoNothing()
    {
        // Arrange
        var container = new NullContainer();

        // Act
        container.Dispose();

        // Assert
    }

    [Fact]
    public void Resolve_ShouldReturnNull()
    {
        // Arrange
        var container = new NullContainer();
        var registrationType = typeof(object);

        // Act
        var result = container.Resolve(registrationType);

        // Assert
        Assert.Null(result);
    }
}