using DependencyInjection.Core;

namespace DependencyInjection.Tests;

public class ContainerTests
{
    private class MockContainerCache : IContainerCache
    {
        private readonly IContainer _foundContainer;

        public MockContainerCache(IContainer foundContainer)
        {
            _foundContainer = foundContainer;
        }

        public IContainer? Find(ReadOnlySpan<char> path) => _foundContainer;
        public void Add(IContainer container) { }
        public void Remove(IContainer container) { }
        public void Clear() { }
    }

    [Fact]
    public void Create_WithNameAndParent_ShouldInitializeContainer()
    {
        // Arrange
        var name = "TestContainer";
        var mockParent = new Mock<IContainer>();
        var mockCache = new MockContainerCache(mockParent.Object);
        var mockConfigurer = new Mock<Action<IContainerConfigurer>>();

        // Act
        var container = Container.Create(name, mockParent.Object, mockCache, mockConfigurer.Object);

        // Assert
        Assert.NotNull(container);
        mockParent.Verify(p => p.AddChild(It.IsAny<IContainer>()), Times.Once);
    }

    [Fact]
    public void Create_WithPath_ShouldInitializeContainer()
    {
        // Arrange
        var path = "/parent/TestContainer".AsSpan();
        var mockParent = new Mock<IContainer>();
        var mockCache = new MockContainerCache(mockParent.Object);
        var mockConfigurer = new Mock<Action<IContainerConfigurer>>();

        // Act
        var container = Container.Create(path, mockCache, mockConfigurer.Object);

        // Assert
        Assert.NotNull(container);
        mockParent.Verify(p => p.AddChild(It.IsAny<IContainer>()), Times.Once);
    }

    [Fact]
    public void Dispose_WithContainer_ShouldDisposeAndRemoveFromCache()
    {
        // Arrange
        var mockContainer = new Mock<IContainer>();
        var mockCache = new MockContainerCache(mockContainer.Object);

        // Act
        Container.Dispose(mockContainer.Object, mockCache);

        // Assert
        mockContainer.Verify(c => c.Dispose(), Times.Once);
    }

    [Fact]
    public void Dispose_WithPath_ShouldDisposeAndRemoveFromCache()
    {
        // Arrange
        var path = "/parent/TestContainer".AsSpan();
        var mockContainer = new Mock<IContainer>();
        var mockCache = new MockContainerCache(mockContainer.Object);

        // Act
        Container.Dispose(path, mockCache);

        // Assert
        mockContainer.Verify(c => c.Dispose(), Times.Once);
    }

    [Fact]
    public void Find_WithPath_ShouldReturnContainer()
    {
        // Arrange
        var path = "/parent/TestContainer";
        var mockContainer = new Mock<IContainer>();
        var mockCache = new MockContainerCache(mockContainer.Object);

        // Act
        var container = Container.Find(path, mockCache);

        // Assert
        Assert.NotNull(container);
        Assert.Equal(mockContainer.Object, container);
    }
}
