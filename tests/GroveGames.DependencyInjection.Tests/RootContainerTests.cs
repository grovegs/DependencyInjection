using GroveGames.DependencyInjection.Caching;
using GroveGames.DependencyInjection.Collections;
using GroveGames.DependencyInjection.Resolution;

namespace GroveGames.DependencyInjection.Tests;

public class RootContainerTests
{
    [Fact]
    public void Constructor_ShouldInitializeRootContainer()
    {
        // Arrange
        var mockCache = new Mock<IContainerCache>();

        static void MockConfigurer(IContainerConfigurer _) { }

        // Act
        var rootContainer = new RootContainer(mockCache.Object, MockConfigurer);

        // Assert
        Assert.NotNull(rootContainer);
        Assert.Equal(string.Empty, rootContainer.Name);
        Assert.IsType<EmptyContainer>(rootContainer.Parent);
    }

    [Fact]
    public void Create_ShouldReturnNewRootContainerWithSharedCache()
    {
        // Arrange
        static void MockConfigurer(IContainerConfigurer _) { }

        // Act
        var rootContainer = RootContainer.Create(MockConfigurer);

        // Assert
        Assert.NotNull(rootContainer);
        Assert.Equal(string.Empty, rootContainer.Name);
        Assert.IsType<EmptyContainer>(rootContainer.Parent);
    }

    [Fact]
    public void AddChild_ShouldDelegateToInternalContainer()
    {
        // Arrange
        var mockCache = new Mock<IContainerCache>();
        var mockChild = new Mock<IContainer>();
        static void MockConfigurer(IContainerConfigurer _) { }
        var rootContainer = new RootContainer(mockCache.Object, MockConfigurer);

        // Act
        rootContainer.AddChild(mockChild.Object);

        // Assert
        var containerField = typeof(RootContainer).GetField("_container", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var internalContainer = (Container)containerField?.GetValue(rootContainer)!;
        var childrenField = typeof(Container).GetField("_children", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var children = (List<IContainer>)childrenField?.GetValue(internalContainer)!;
        Assert.Contains(mockChild.Object, children);
    }

    [Fact]
    public void RemoveChild_ShouldDelegateToInternalContainer()
    {
        // Arrange
        var mockCache = new Mock<IContainerCache>();
        var mockChild = new Mock<IContainer>();
        static void MockConfigurer(IContainerConfigurer _) { }
        var rootContainer = new RootContainer(mockCache.Object, MockConfigurer);

        rootContainer.AddChild(mockChild.Object);

        // Act
        rootContainer.RemoveChild(mockChild.Object);

        // Assert
        var containerField = typeof(RootContainer).GetField("_container", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var internalContainer = (Container)containerField?.GetValue(rootContainer)!;
        var childrenField = typeof(Container).GetField("_children", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var children = (List<IContainer>)childrenField?.GetValue(internalContainer)!;
        Assert.DoesNotContain(mockChild.Object, children);
    }

    [Fact]
    public void Resolve_ShouldDelegateToInternalContainer()
    {
        // Arrange
        var mockCache = new Mock<IContainerCache>();
        static void MockConfigurer(IContainerConfigurer _) { }
        var rootContainer = new RootContainer(mockCache.Object, MockConfigurer);

        var mockObject = new object();
        var mockResolver = new Mock<IContainerResolver>();
        mockResolver.Setup(r => r.Resolve(It.IsAny<Type>())).Returns(mockObject);

        var containerField = typeof(RootContainer).GetField("_container", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var internalContainer = (Container)containerField?.GetValue(rootContainer)!;
        var resolverField = typeof(Container).GetField("_resolver", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        resolverField?.SetValue(internalContainer, mockResolver.Object);

        // Act
        var result = rootContainer.Resolve(typeof(object));

        // Assert
        Assert.Equal(mockObject, result);
    }

    [Fact]
    public void Dispose_ShouldDelegateToInternalContainer()
    {
        // Arrange
        var mockCache = new Mock<IContainerCache>();
        static void MockConfigurer(IContainerConfigurer _) { }
        var rootContainer = new RootContainer(mockCache.Object, MockConfigurer);

        var mockDisposables = new Mock<IDisposableCollection>();
        var containerField = typeof(RootContainer).GetField("_container", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var internalContainer = (Container)containerField?.GetValue(rootContainer)!;
        var disposablesField = typeof(Container).GetField("_disposables", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        disposablesField?.SetValue(internalContainer, mockDisposables.Object);

        // Act
        rootContainer.Dispose();

        // Assert
        mockDisposables.Verify(d => d.Dispose(), Times.Once);
    }
}