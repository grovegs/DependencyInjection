using DependencyInjection.Core;
using DependencyInjection.Resolution;

namespace DependencyInjection.Tests.Core;

public class ContainerTests
{
    [Fact]
    public void Constructor_ShouldInitializeContainerWithCorrectValues()
    {
        // Arrange
        var mockResolver = new Mock<IContainerResolver>();
        var mockDisposables = new Mock<IDisposableCollection>();
        var mockParent = new Mock<IContainer>();

        // Act
        var container = new DependencyInjection.Core.Container("TestContainer", mockResolver.Object, mockDisposables.Object, mockParent.Object);

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
        var container = new DependencyInjection.Core.Container("TestContainer", mockResolver.Object, mockDisposables.Object, mockParent.Object);
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
        var container = new DependencyInjection.Core.Container("TestContainer", mockResolver.Object, mockDisposables.Object, mockParent.Object);
        var mockChild = new Mock<IContainer>();

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
        var container = new DependencyInjection.Core.Container("TestContainer", mockResolver.Object, mockDisposables.Object, mockParent.Object);
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
        var container = new DependencyInjection.Core.Container("TestContainer", mockResolver.Object, mockDisposables.Object, mockParent.Object);
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
        var container = new DependencyInjection.Core.Container("TestContainer", mockResolver.Object, mockDisposables.Object, mockParent.Object);
        var child1 = new Mock<IContainer>();
        var child2 = new Mock<IContainer>();
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
        var container = new DependencyInjection.Core.Container("TestContainer", mockResolver.Object, mockDisposables.Object, mockParent.Object);

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
        var container = new DependencyInjection.Core.Container("TestContainer", mockResolver.Object, mockDisposables.Object, mockParent.Object);
        var testType = typeof(string);
        mockResolver.Setup(r => r.Resolve(testType)).Returns("TestString");

        // Act
        var result = container.Resolve(testType);

        // Assert
        Assert.Equal("TestString", result);
        mockResolver.Verify(r => r.Resolve(testType), Times.Once);
    }
}
