using DependencyInjection.Caching;

namespace DependencyInjection.Tests;

public class DITests
{
    [Fact]
    public void CreateContainer_WithName_ShouldReturnContainer()
    {
        // Arrange
        var name = "TestContainer";
        var parent = new Mock<IContainer>();
        var configurer = new Mock<Action<IContainerConfigurer>>();

        // Act
        var container = DI.CreateContainer(name, parent.Object, configurer.Object);

        // Assert
        Assert.NotNull(container);
        ContainerCache.Shared.Clear();
    }

    [Fact]
    public void CreateContainer_WithPath_ShouldReturnContainer()
    {
        // Arrange
        var path = "Test/Path".AsSpan();
        var rootMock = new Mock<IContainer>();
        var configurer = new Mock<Action<IContainerConfigurer>>();
        var parent = DI.CreateContainer("Test", rootMock.Object, configurer.Object);

        // Act
        var container = DI.CreateContainer(path, configurer.Object);

        // Assert
        Assert.NotNull(parent);
        Assert.NotNull(container);
        ContainerCache.Shared.Clear();
    }

    [Fact]
    public void DisposeContainer_WithContainer_ShouldDisposeContainer()
    {
        // Arrange
        var container = new Mock<IContainer>();

        // Act
        DI.DisposeContainer(container.Object);

        // Assert
        container.Verify(c => c.Dispose(), Times.Once);
    }

    [Fact]
    public void DisposeContainer_WithPath_ShouldDisposeContainer()
    {
        // Arrange
        var path = "Test/Path".AsSpan();
        var rootMock = new Mock<IContainer>();
        var configurer = new Mock<Action<IContainerConfigurer>>();
        var parent = DI.CreateContainer("Test", rootMock.Object, configurer.Object);
        var container = DI.CreateContainer(path, configurer.Object);

        // Act
        DI.DisposeContainer(path);

        // Assert
        Assert.Null(ContainerCache.Shared.Find(path));
        Assert.NotNull(container);
        ContainerCache.Shared.Clear();
    }

    [Fact]
    public void FindContainer_WithPath_ShouldReturnContainer()
    {
        // Arrange
        var path = "Test/Path".AsSpan();
        var rootMock = new Mock<IContainer>();
        var configurer = new Mock<Action<IContainerConfigurer>>();
        var parent = DI.CreateContainer("Test", rootMock.Object, configurer.Object);
        var container = DI.CreateContainer(path, configurer.Object);

        // Act
        var foundContainer = DI.FindContainer(path);

        // Assert
        Assert.NotNull(container);
        Assert.Equal(foundContainer, container);
        ContainerCache.Shared.Clear();
    }
}
