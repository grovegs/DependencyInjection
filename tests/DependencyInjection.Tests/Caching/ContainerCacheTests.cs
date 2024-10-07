using DependencyInjection.Caching;

namespace DependencyInjection.Tests.Caching;

public class ContainerCacheTests
{
    [Fact]
    public void Shared_ShouldReturnSingletonInstance()
    {
        // Act
        var instance1 = ContainerCache.Shared;
        var instance2 = ContainerCache.Shared;

        // Assert
        Assert.Same(instance1, instance2);
    }

    [Fact]
    public void Find_ShouldReturnContainer_WhenPathIsValid()
    {
        // Arrange
        var rootContainer = new Mock<IContainer>();
        rootContainer.SetupGet(c => c.Name).Returns(string.Empty);
        rootContainer.SetupGet(c => c.Parent).Returns((IContainer)null!);
        var childContainer = new Mock<IContainer>();
        childContainer.SetupGet(c => c.Name).Returns("child");
        childContainer.SetupGet(c => c.Parent).Returns(rootContainer.Object);
        var grandchildContainer = new Mock<IContainer>();
        grandchildContainer.SetupGet(c => c.Name).Returns("grandchild");
        grandchildContainer.SetupGet(c => c.Parent).Returns(childContainer.Object);
        var containerCache = new ContainerCache();
        containerCache.Add(rootContainer.Object);
        containerCache.Add(childContainer.Object);
        containerCache.Add(grandchildContainer.Object);

        // Act
        var result = containerCache.Find("/child/grandchild");

        // Assert
        Assert.Equal(grandchildContainer.Object, result);
    }

    [Fact]
    public void Find_ShouldReturnEmptyNameContainer_WhenPathIsEmpty()
    {
        // Arrange
        var rootContainer = new Mock<IContainer>();
        rootContainer.SetupGet(c => c.Name).Returns(string.Empty);
        rootContainer.SetupGet(c => c.Parent).Returns((IContainer)null!);
        var containerCache = new ContainerCache();
        containerCache.Add(rootContainer.Object);

        // Act
        var result = containerCache.Find(string.Empty);

        // Assert
        Assert.Equal(rootContainer.Object, result);
    }

    [Fact]
    public void Find_ShouldReturnEmptyNameContainer_WhenPathIsSlash()
    {
        // Arrange
        var rootContainer = new Mock<IContainer>();
        rootContainer.SetupGet(c => c.Name).Returns(string.Empty);
        rootContainer.SetupGet(c => c.Parent).Returns((IContainer)null!);
        var containerCache = new ContainerCache();
        containerCache.Add(rootContainer.Object);

        // Act
        var result = containerCache.Find("/");

        // Assert
        Assert.Equal(rootContainer.Object, result);
    }

    [Fact]
    public void Find_ShouldReturnNull_WhenPathDoesNotMatch()
    {
        // Arrange
        var rootContainer = new Mock<IContainer>();
        rootContainer.SetupGet(c => c.Name).Returns("root");
        rootContainer.SetupGet(c => c.Parent).Returns((IContainer)null!);
        var containerCache = new ContainerCache();
        containerCache.Add(rootContainer.Object);

        // Act
        var result = containerCache.Find("nonexistent/path");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void Add_ShouldAddContainerToCache()
    {
        // Arrange
        var container = new Mock<IContainer>();
        container.SetupGet(c => c.Name).Returns("test");
        var containerCache = new ContainerCache();

        // Act
        containerCache.Add(container.Object);

        // Assert
        var result = containerCache.Find("test");
        Assert.Equal(container.Object, result);
    }

    [Fact]
    public void Remove_ShouldRemoveContainerFromCache()
    {
        // Arrange
        var container = new Mock<IContainer>();
        container.SetupGet(c => c.Name).Returns("test");
        var containerCache = new ContainerCache();
        containerCache.Add(container.Object);

        // Act
        containerCache.Remove(container.Object);

        // Assert
        var result = containerCache.Find("test");
        Assert.Null(result);
    }

    [Fact]
    public void Clear_ShouldRemoveAllContainersFromCache()
    {
        // Arrange
        var container1 = new Mock<IContainer>();
        container1.SetupGet(c => c.Name).Returns("test1");
        var container2 = new Mock<IContainer>();
        container2.SetupGet(c => c.Name).Returns("test2");
        var containerCache = new ContainerCache();
        containerCache.Add(container1.Object);
        containerCache.Add(container2.Object);

        // Act
        containerCache.Clear();

        // Assert
        var result1 = containerCache.Find("test1");
        var result2 = containerCache.Find("test2");
        Assert.Null(result1);
        Assert.Null(result2);
    }
}
