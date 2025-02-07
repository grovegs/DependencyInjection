using System.Reflection;

using GroveGames.DependencyInjection.Caching;
using GroveGames.DependencyInjection.Collections;
using GroveGames.DependencyInjection.Resolution;

namespace GroveGames.DependencyInjection.Tests;

public class ContainerTests
{
    private Container CreateContainer(
        string name = "TestContainer",
        Mock<IContainerResolver>? resolverMock = null,
        Mock<IContainerCache>? cacheMock = null,
        Mock<IDisposableCollection>? disposablesMock = null,
        Mock<IContainer>? parentMock = null
    )
    {
        resolverMock ??= new Mock<IContainerResolver>();
        cacheMock ??= new Mock<IContainerCache>();
        disposablesMock ??= new Mock<IDisposableCollection>();
        parentMock ??= new Mock<IContainer>();
        parentMock.Setup(p => p.AddChild(It.IsAny<IContainer>()));

        return new Container(
            name,
            parentMock.Object,
            resolverMock.Object,
            cacheMock.Object,
            disposablesMock.Object
        );
    }

    [Fact]
    public void Constructor_ShouldInitializeContainer()
    {
        // Arrange
        var mockParentContainer = new Mock<IContainer>();
        var mockCache = new Mock<IContainerCache>();

        // Act
        var container = CreateContainer(parentMock: mockParentContainer, cacheMock: mockCache);

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
        var container = CreateContainer();
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
        var container = CreateContainer();
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
        var container = CreateContainer();
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
        var mockDisposables = new Mock<IDisposableCollection>();
        var mockCache = new Mock<IContainerCache>();
        var mockParentContainer = new Mock<IContainer>();

        var container = CreateContainer(disposablesMock: mockDisposables, cacheMock: mockCache, parentMock: mockParentContainer);

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
        var container = CreateContainer(resolverMock: mockResolver);

        var mockObject = new object();
        mockResolver.Setup(r => r.Resolve(It.IsAny<Type>())).Returns(mockObject);

        // Act
        var result = container.Resolve(typeof(object));

        // Assert
        Assert.Equal(mockObject, result);
    }
}