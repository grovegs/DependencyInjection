using System.Collections.ObjectModel;
using DependencyInjection.Core;

namespace DependencyInjection.Tests;

public sealed class ContainerCacheTests
{
    [Fact]
    public void Find_ReturnsCorrectContainer_WhenExactPathIsFound_IgnoringCase()
    {
        // Arrange
        ContainerCache.Clear();

        var rootContainer = new Mock<IContainer>();
        rootContainer.Setup(c => c.Name).Returns("root");
        rootContainer.Setup(c => c.Parent).Returns((IContainer?)null);

        var childContainer = new Mock<IContainer>();
        childContainer.Setup(c => c.Name).Returns("child");
        childContainer.Setup(c => c.Parent).Returns(rootContainer.Object);

        var grandChildContainer = new Mock<IContainer>();
        grandChildContainer.Setup(c => c.Name).Returns("grandchild");
        grandChildContainer.Setup(c => c.Parent).Returns(childContainer.Object);

        ContainerCache.Add(grandChildContainer.Object);
        ContainerCache.Add(childContainer.Object);
        ContainerCache.Add(rootContainer.Object);

        // Act
        var result = ContainerCache.Find("ROOT/Child/GRANDCHILD");

        // Assert
        Assert.Equal(grandChildContainer.Object, result);
    }

    [Fact]
    public void Find_ReturnsNull_WhenPathDoesNotExist()
    {
        // Arrange
        ContainerCache.Clear();

        var rootContainer = new Mock<IContainer>();
        rootContainer.Setup(c => c.Name).Returns("root");
        rootContainer.Setup(c => c.Parent).Returns((IContainer?)null);

        ContainerCache.Add(rootContainer.Object);

        // Act
        var result = ContainerCache.Find("root/nonexistent");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void Find_ReturnsNull_WhenSegmentDoesNotMatch()
    {
        // Arrange
        ContainerCache.Clear();

        var rootContainer = new Mock<IContainer>();
        rootContainer.Setup(c => c.Name).Returns("root");
        rootContainer.Setup(c => c.Parent).Returns((IContainer?)null);

        var childContainer = new Mock<IContainer>();
        childContainer.Setup(c => c.Name).Returns("child");
        childContainer.Setup(c => c.Parent).Returns(rootContainer.Object);

        ContainerCache.Add(childContainer.Object);
        ContainerCache.Add(rootContainer.Object);

        // Act
        var result = ContainerCache.Find("root/wrongChild");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void Find_ReturnsContainer_WhenOnlyOneSegmentMatches_IgnoringCase()
    {
        // Arrange
        ContainerCache.Clear();

        var rootContainer = new Mock<IContainer>();
        rootContainer.Setup(c => c.Name).Returns("root");
        rootContainer.Setup(c => c.Parent).Returns((IContainer?)null);

        ContainerCache.Add(rootContainer.Object);

        // Act
        var result = ContainerCache.Find("ROOT");

        // Assert
        Assert.Equal(rootContainer.Object, result);
    }

    [Fact]
    public void Find_ReturnsNull_WhenContainerListIsEmpty()
    {
        // Arrange
        ContainerCache.Clear();

        // Act
        var result = ContainerCache.Find("root");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void Add_AddsContainerToList()
    {
        // Arrange
        ContainerCache.Clear();

        var container = new Mock<IContainer>();
        ContainerCache.Add(container.Object);

        // Act
        var result = ContainerCache.Find(container.Object.Name);

        // Assert
        Assert.Equal(container.Object, result);
    }

    [Fact]
    public void Remove_RemovesContainerFromList()
    {
        // Arrange
        ContainerCache.Clear();

        var container = new Mock<IContainer>();
        ContainerCache.Add(container.Object);
        ContainerCache.Remove(container.Object);

        // Act
        var result = ContainerCache.Find(container.Object.Name);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void Clear_RemovesAllContainers()
    {
        // Arrange
        ContainerCache.Clear();

        var container = new Mock<IContainer>();
        ContainerCache.Add(container.Object);

        // Act
        ContainerCache.Clear();
        var result = ContainerCache.Find(container.Object.Name);

        // Assert
        Assert.Null(result);
    }
}
