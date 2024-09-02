using DependencyInjection.Core;
using DependencyInjection.Resolution;
using DependencyInjection.Tests.Fakes;

namespace DependencyInjection.Tests;

public sealed class ContainerTests
{
    [Fact]
    public void Build_Container_InitializableClassShouldBeInitialized()
    {
        var containerBuilder = new ContainerBuilder("Container", Container.Empty);
        containerBuilder.AddSingleton(typeof(IInitializableClass), typeof(InitializableClass));
        var container = containerBuilder.Build();
        var initializableInstance = container.Resolve(typeof(IInitializableClass)) as IInitializableClass;
        Assert.True(initializableInstance?.Initialized);
    }

    [Fact]
    public void Dispose_ParentContainer_ChildContainerResolverShouldReturnNull()
    {
        var parentBuilder = new ContainerBuilder("Parent", Container.Empty);
        parentBuilder.AddSingleton(typeof(ZeroParameterClass));
        var parentContainer = parentBuilder.Build();
        var childBuilder = new ContainerBuilder("Child", parentContainer);
        childBuilder.AddSingleton(typeof(OneParameterClass));
        var childContainer = childBuilder.Build();
        var childOfChildBuilder = new ContainerBuilder("ChildOfChild", childContainer);
        childOfChildBuilder.AddSingleton(typeof(ClassWithInjectableMethod));
        var childOfChildContainer = childOfChildBuilder.Build();
        parentContainer.Dispose();
        Assert.Null(childOfChildContainer.Resolve(typeof(ClassWithInjectableMethod)));
    }

    [Fact]
    public void Dispose_Container_ResolvedDisposableShouldBeDisposed()
    {
        var containerBuilder = new ContainerBuilder("Container", Container.Empty);
        containerBuilder.AddSingleton(typeof(IDisposableClass), typeof(DisposableClass));
        var container = containerBuilder.Build();
        var disposableInstance = container.Resolve(typeof(IDisposableClass)) as IDisposableClass;
        container.Dispose();
        Assert.True(disposableInstance?.Disposed);
    }

    [Fact]
    public void Dispose_ParentContainer_ResolvedChildDisposableShouldBeDisposed()
    {
        var parentBuilder = new ContainerBuilder("Parent", Container.Empty);
        parentBuilder.AddSingleton(typeof(ZeroParameterClass));
        var parentContainer = parentBuilder.Build();
        var childBuilder = new ContainerBuilder("Child", parentContainer);
        childBuilder.AddSingleton(typeof(IDisposableClass), typeof(DisposableClass));
        var childContainer = childBuilder.Build();
        var disposableInstance = childContainer.Resolve(typeof(IDisposableClass)) as IDisposableClass;
        parentContainer.Dispose();
        Assert.True(disposableInstance?.Disposed);
    }

    [Fact]
    public void Resolve_ParentImplementationFromChild_ChildContainerResolverShouldReturnInstanceOfParentImplementation()
    {
        var parentBuilder = new ContainerBuilder("Parent", Container.Empty);
        var zeroParameterClass = new ZeroParameterClass();
        parentBuilder.AddInstance(zeroParameterClass);
        var parentContainer = parentBuilder.Build();
        var childBuilder = new ContainerBuilder("Child", parentContainer);
        var childContainer = childBuilder.Build();
        var expected = zeroParameterClass;
        var actual = childContainer.Resolve(typeof(ZeroParameterClass));
        Assert.Same(expected, actual);
    }

    [Fact]
    public void Container_Name_ShouldReturnInitializedName()
    {
        // Arrange
        var expectedName = "TestContainer";
        var mockResolver = new Mock<IContainerResolver>();
        var mockDisposables = new Mock<IDisposableCollection>();
        var mockChildren = new Mock<IList<IContainer>>();
        var mockParent = new Mock<IContainer>();

        // Act
        var container = new Container(expectedName, mockResolver.Object, mockDisposables.Object, mockChildren.Object, mockParent.Object);

        // Assert
        Assert.Equal(expectedName, container.Name);
    }

    [Fact]
    public void Container_Parent_ShouldReturnInitializedParent()
    {
        // Arrange
        var mockResolver = new Mock<IContainerResolver>();
        var mockDisposables = new Mock<IDisposableCollection>();
        var mockChildren = new Mock<IList<IContainer>>();
        var mockParent = new Mock<IContainer>();

        // Act
        var container = new Container("TestContainer", mockResolver.Object, mockDisposables.Object, mockChildren.Object, mockParent.Object);

        // Assert
        Assert.Same(mockParent.Object, container.Parent);
    }

    [Fact]
    public void Container_Parent_ShouldReturnNullIfNoParentProvided()
    {
        // Arrange
        var mockResolver = new Mock<IContainerResolver>();
        var mockDisposables = new Mock<IDisposableCollection>();
        var mockChildren = new Mock<IList<IContainer>>();

        // Act
        var container = new Container("TestContainer", mockResolver.Object, mockDisposables.Object, mockChildren.Object, Container.Empty);

        // Assert
        Assert.Null(container.Parent);
    }
}
