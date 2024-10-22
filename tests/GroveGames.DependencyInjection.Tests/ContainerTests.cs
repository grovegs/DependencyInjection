using System.Reflection;
using GroveGames.DependencyInjection.Caching;
using GroveGames.DependencyInjection.Collections;
using GroveGames.DependencyInjection.Resolution;

namespace GroveGames.DependencyInjection.Tests;

public class ContainerTests
{
    [Fact]
    public void Constructor_ShouldInitializeContainer()
    {
        // Arrange
        var mockResolver = new Mock<IContainerResolver>();
        var mockCache = new Mock<IContainerCache>();
        var mockDisposables = new Mock<IDisposableCollection>();
        var mockParentContainer = new Mock<IContainer>();
        mockParentContainer.Setup(p => p.AddChild(It.IsAny<IContainer>()));

        // Act
        var container = new Container(
            "TestContainer",
            mockParentContainer.Object,
            mockResolver.Object,
            mockCache.Object,
            mockDisposables.Object,
            _ => { }
        );

        // Assert
        Assert.Equal("TestContainer", container.Name);
        Assert.Equal(mockParentContainer.Object, container.Parent);
        mockParentContainer.Verify(p => p.AddChild(container), Times.Once);
        mockCache.Verify(c => c.Add(container), Times.Once);
    }

    [Fact]
    public void AddChild_ShouldAddChildContainer()
    {
        // Arrange
        var mockResolver = new Mock<IContainerResolver>();
        var mockCache = new Mock<IContainerCache>();
        var mockDisposables = new Mock<IDisposableCollection>();
        var mockParentContainer = new Mock<IContainer>();
        var container = new Container(
            "TestContainer",
            mockParentContainer.Object,
            mockResolver.Object,
            mockCache.Object,
            mockDisposables.Object,
            _ => { }
        );

        var childMock = new Mock<IContainer>();
        childMock.Setup(c => c.Name).Returns("ChildContainer");

        // Act
        container.AddChild(childMock.Object);

        // Assert
        var childrenField = typeof(Container).GetField("_children", BindingFlags.NonPublic | BindingFlags.Instance);
        var children = childrenField?.GetValue(container) as List<IContainer>;
        Assert.Contains(childMock.Object, children!);
    }

    [Fact]
    public void AddChild_ShouldThrowArgumentException_WhenDuplicateChildIsAdded()
    {
        // Arrange
        var mockResolver = new Mock<IContainerResolver>();
        var mockCache = new Mock<IContainerCache>();
        var mockDisposables = new Mock<IDisposableCollection>();
        var mockParentContainer = new Mock<IContainer>();
        var container = new Container(
            "TestContainer",
            mockParentContainer.Object,
            mockResolver.Object,
            mockCache.Object,
            mockDisposables.Object,
            _ => { }
        );

        var childMock = new Mock<IContainer>();
        childMock.Setup(c => c.Name).Returns("ChildContainer");
        container.AddChild(childMock.Object);

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => container.AddChild(childMock.Object));
        Assert.Contains("A child container with the same name", ex.Message);
    }

    [Fact]
    public void RemoveChild_ShouldRemoveChildContainer()
    {
        // Arrange
        var mockResolver = new Mock<IContainerResolver>();
        var mockCache = new Mock<IContainerCache>();
        var mockDisposables = new Mock<IDisposableCollection>();
        var mockParentContainer = new Mock<IContainer>();
        var container = new Container(
            "TestContainer",
            mockParentContainer.Object,
            mockResolver.Object,
            mockCache.Object,
            mockDisposables.Object,
            _ => { }
        );

        var childMock = new Mock<IContainer>();
        childMock.Setup(c => c.Name).Returns("ChildContainer");
        container.AddChild(childMock.Object);

        // Act
        container.RemoveChild(childMock.Object);

        // Assert
        var childrenField = typeof(Container).GetField("_children", BindingFlags.NonPublic | BindingFlags.Instance);
        var children = childrenField?.GetValue(container) as List<IContainer>;
        Assert.DoesNotContain(childMock.Object, children!);
    }

    [Fact]
    public void Dispose_ShouldDisposeAllResources()
    {
        // Arrange
        var mockResolver = new Mock<IContainerResolver>();
        var mockCache = new Mock<IContainerCache>();
        var mockDisposables = new Mock<IDisposableCollection>();
        var mockParentContainer = new Mock<IContainer>();
        var container = new Container(
            "TestContainer",
            mockParentContainer.Object,
            mockResolver.Object,
            mockCache.Object,
            mockDisposables.Object,
            _ => { }
        );

        // Act
        container.Dispose();

        // Assert
        mockDisposables.Verify(d => d.Dispose(), Times.Once);
        mockCache.Verify(c => c.Remove(container), Times.Once);
        mockParentContainer.Verify(p => p.RemoveChild(container), Times.Once);
    }

    [Fact]
    public void Resolve_ShouldUseResolverToResolveType()
    {
        // Arrange
        var mockResolver = new Mock<IContainerResolver>();
        var mockCache = new Mock<IContainerCache>();
        var mockDisposables = new Mock<IDisposableCollection>();
        var mockParentContainer = new Mock<IContainer>();
        var container = new Container(
            "TestContainer",
            mockParentContainer.Object,
            mockResolver.Object,
            mockCache.Object,
            mockDisposables.Object,
            _ => { }
        );

        var mockObject = new object();
        mockResolver.Setup(r => r.Resolve(It.IsAny<Type>())).Returns(mockObject);

        // Act
        var result = container.Resolve(typeof(object));

        // Assert
        Assert.Equal(mockObject, result);
    }

    [Fact]
    public void Create_WithNameAndParent_UsingDefaultCache_ShouldCreateContainer()
    {
        // Arrange
        var mockParent = new Mock<IContainer>();

        static void MockConfigurer(IContainerConfigurer _) { }

        // Act
        var result = Container.Create("TestContainer", mockParent.Object, MockConfigurer);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("TestContainer", result.Name);
        Assert.Equal(mockParent.Object, result.Parent);
    }

    [Fact]
    public void Dispose_WithPath_ShouldDoNothingIfContainerNotFound()
    {
        // Arrange
        var path = "/parent/container".AsSpan();

        // Act
        Container.Dispose(path);

        // Assert
        // Nothing to verify since the container was not found, but we can ensure no exception is thrown.
    }
}
