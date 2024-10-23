namespace GroveGames.DependencyInjection.Tests;

public class NullContainerTests
{
    [Fact]
    public void Name_ShouldReturnEmptyString()
    {
        // Arrange
        var nullContainer = new NullContainer();

        // Act
        var name = nullContainer.Name;

        // Assert
        Assert.Equal(string.Empty, name);
    }

    [Fact]
    public void Parent_ShouldReturnNull()
    {
        // Arrange
        var nullContainer = new NullContainer();

        // Act
        var parent = nullContainer.Parent;

        // Assert
        Assert.Null(parent);
    }

    [Fact]
    public void AddChild_ShouldDoNothing()
    {
        // Arrange
        var nullContainer = new NullContainer();
        var mockChild = new Mock<IContainer>();

        // Act
        nullContainer.AddChild(mockChild.Object);

        // Assert
    }

    [Fact]
    public void RemoveChild_ShouldDoNothing()
    {
        // Arrange
        var nullContainer = new NullContainer();
        var mockChild = new Mock<IContainer>();

        // Act
        nullContainer.RemoveChild(mockChild.Object);

        // Assert
    }

    [Fact]
    public void Dispose_ShouldDoNothing()
    {
        // Arrange
        var nullContainer = new NullContainer();

        // Act
        nullContainer.Dispose();

        // Assert
    }

    [Fact]
    public void Resolve_ShouldReturnNull()
    {
        // Arrange
        var nullContainer = new NullContainer();
        var registrationType = typeof(object);

        // Act
        var result = nullContainer.Resolve(registrationType);

        // Assert
        Assert.Null(result);
    }
}