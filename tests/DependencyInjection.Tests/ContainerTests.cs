using DependencyInjection.Caching;
using DependencyInjection.Collections;
using DependencyInjection.Resolution;

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

    [Fact]
    public void Constructor_ShouldInitializeContainerWithCorrectValues()
    {
        // Arrange
        var mockResolver = new Mock<IContainerResolver>();
        var mockDisposables = new Mock<IDisposableCollection>();
        var mockParent = new Mock<IContainer>();

        // Act
        var container = new Container("TestContainer", mockResolver.Object, mockDisposables.Object, mockParent.Object);

        // Assert
        Assert.Equal("TestContainer", container.Name);
        Assert.Equal(mockParent.Object, container.Parent);
    }

    [Fact]
    public void AddChild_ShouldAddChildContainer()
    {
        // Arrange
        var mockResolver = new Mock<IContainerResolver>();
        var mockDisposables = new Mock<IDisposableCollection>();
        var mockParent = new Mock<IContainer>();
        var container = new Container("TestContainer", mockResolver.Object, mockDisposables.Object, mockParent.Object);
        var mockChild = new Mock<IContainer>();

        // Act
        container.AddChild(mockChild.Object);

        // Assert
        container.RemoveChild(mockChild.Object);
        mockChild.Verify(c => c.Dispose(), Times.Never);
    }

    [Fact]
    public void AddChild_ShouldThrowExceptionWhenAddingAlreadyAddedContainer()
    {
        // Arrange
        var mockResolver = new Mock<IContainerResolver>();
        var mockDisposables = new Mock<IDisposableCollection>();
        var mockParent = new Mock<IContainer>();
        var container = new Container("TestContainer", mockResolver.Object, mockDisposables.Object, mockParent.Object);
        var mockChild = new Mock<IContainer>();
        mockChild.SetupGet(c => c.Name).Returns("Child");

        // Act
        container.AddChild(mockChild.Object);

        // Assert
        Assert.Throws<ArgumentException>(() => container.AddChild(mockChild.Object));
    }

    [Fact]
    public void AddChild_ShouldThrowExceptionWhenAddingAlreadyAddedContainerWithSameName()
    {
        // Arrange
        var mockResolver = new Mock<IContainerResolver>();
        var mockDisposables = new Mock<IDisposableCollection>();
        var mockParent = new Mock<IContainer>();
        var container = new Container("TestContainer", mockResolver.Object, mockDisposables.Object, mockParent.Object);
        var mockChild1 = new Mock<IContainer>();
        mockChild1.SetupGet(c => c.Name).Returns("Child");
        var mockChild2 = new Mock<IContainer>();
        mockChild2.SetupGet(c => c.Name).Returns("Child");

        // Act
        container.AddChild(mockChild1.Object);

        // Assert
        Assert.Throws<ArgumentException>(() => container.AddChild(mockChild2.Object));
    }

    [Fact]
    public void RemoveChild_ShouldRemoveChildContainer()
    {
        // Arrange
        var mockResolver = new Mock<IContainerResolver>();
        var mockDisposables = new Mock<IDisposableCollection>();
        var mockParent = new Mock<IContainer>();
        var container = new Container("TestContainer", mockResolver.Object, mockDisposables.Object, mockParent.Object);
        var mockChild = new Mock<IContainer>();

        // Act
        container.AddChild(mockChild.Object);
        container.RemoveChild(mockChild.Object);

        // Assert
        mockChild.Verify(c => c.Dispose(), Times.Never);
    }

    [Fact]
    public void Dispose_ShouldDisposeChildrenAndClearResolver()
    {
        // Arrange
        var mockResolver = new Mock<IContainerResolver>();
        var mockDisposables = new Mock<IDisposableCollection>();
        var mockParent = new Mock<IContainer>();
        var container = new Container("TestContainer", mockResolver.Object, mockDisposables.Object, mockParent.Object);
        var child1 = new Mock<IContainer>();
        child1.SetupGet(c => c.Name).Returns("Child1");
        var child2 = new Mock<IContainer>();
        child2.SetupGet(c => c.Name).Returns("Child2");
        container.AddChild(child1.Object);
        container.AddChild(child2.Object);

        // Act
        container.Dispose();

        // Assert
        mockDisposables.Verify(d => d.Dispose(), Times.Once);
        child1.Verify(c => c.Dispose(), Times.Once);
        child2.Verify(c => c.Dispose(), Times.Once);
        mockResolver.Verify(r => r.Clear(), Times.Once);
        mockParent.Verify(p => p.RemoveChild(container), Times.Once);
    }

    [Fact]
    public void Dispose_ShouldOnlyDisposeOnce()
    {
        // Arrange
        var mockResolver = new Mock<IContainerResolver>();
        var mockDisposables = new Mock<IDisposableCollection>();
        var mockParent = new Mock<IContainer>();
        var container = new Container("TestContainer", mockResolver.Object, mockDisposables.Object, mockParent.Object);

        // Act
        container.Dispose();
        container.Dispose();

        // Assert
        mockDisposables.Verify(d => d.Dispose(), Times.Once);
        mockResolver.Verify(r => r.Clear(), Times.Once);
        mockParent.Verify(p => p.RemoveChild(container), Times.Once);
    }

    [Fact]
    public void Resolve_ShouldCallResolverToResolveType()
    {
        // Arrange
        var mockResolver = new Mock<IContainerResolver>();
        var mockDisposables = new Mock<IDisposableCollection>();
        var mockParent = new Mock<IContainer>();
        var container = new Container("TestContainer", mockResolver.Object, mockDisposables.Object, mockParent.Object);
        var testType = typeof(string);
        mockResolver.Setup(r => r.Resolve(testType)).Returns("TestString");

        // Act
        var result = container.Resolve(testType);

        // Assert
        Assert.Equal("TestString", result);
        mockResolver.Verify(r => r.Resolve(testType), Times.Once);
    }
}
