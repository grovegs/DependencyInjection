using DependencyInjection.Core;
using DependencyInjection.Tests.Fakes;

namespace DependencyInjection.Tests;

public sealed class ContainerTests
{
    [Fact]
    public void Build_Container_InitializableClassShouldBeInitialized()
    {
        var containerBuilder = new ContainerBuilder(Containers.Root);
        containerBuilder.AddSingleton(typeof(IInitializableClass), typeof(InitializableClass));
        var container = containerBuilder.Build();
        var initializableInstance = (IInitializableClass)container.Resolve(typeof(IInitializableClass));
        Assert.True(initializableInstance.Initialized);
    }

    [Fact]
    public void Dispose_ParentContainer_ChildContainerResolverShouldReturnNull()
    {
        var parentBuilder = new ContainerBuilder(Containers.Root);
        parentBuilder.AddSingleton(typeof(ZeroParameterClass));
        var parentContainer = parentBuilder.Build();
        var childBuilder = new ContainerBuilder(parentContainer);
        childBuilder.AddSingleton(typeof(OneParameterClass));
        var childContainer = childBuilder.Build();
        var childOfChildBuilder = new ContainerBuilder(childContainer);
        childOfChildBuilder.AddSingleton(typeof(ClassWithInjectableMethod));
        var childOfChildContainer = childOfChildBuilder.Build();
        parentContainer.Dispose();
        Assert.Null(childOfChildContainer.Resolve(typeof(ClassWithInjectableMethod)));
    }

    [Fact]
    public void Dispose_Container_ResolvedDisposableShouldBeDisposed()
    {
        var containerBuilder = new ContainerBuilder(Containers.Root);
        containerBuilder.AddSingleton(typeof(IDisposableClass), typeof(DisposableClass));
        var container = containerBuilder.Build();
        var disposableInstance = (IDisposableClass)container.Resolve(typeof(IDisposableClass));
        container.Dispose();
        Assert.True(disposableInstance.Disposed);
    }

    [Fact]
    public void Dispose_ParentContainer_ResolvedChildDisposableShouldBeDisposed()
    {
        var parentBuilder = new ContainerBuilder(Containers.Root);
        parentBuilder.AddSingleton(typeof(ZeroParameterClass));
        var parentContainer = parentBuilder.Build();
        var childBuilder = new ContainerBuilder(parentContainer);
        childBuilder.AddSingleton(typeof(IDisposableClass), typeof(DisposableClass));
        var childContainer = childBuilder.Build();
        var disposableInstance = (IDisposableClass)childContainer.Resolve(typeof(IDisposableClass));
        parentContainer.Dispose();
        Assert.True(disposableInstance.Disposed);
    }

    [Fact]
    public void Resolve_ParentImplementationFromChild_ChildContainerResolverShouldReturnInstanceOfParentImplementation()
    {
        var parentBuilder = new ContainerBuilder(Containers.Root);
        var zeroParameterClass = new ZeroParameterClass();
        parentBuilder.AddInstance(zeroParameterClass);
        var parentContainer = parentBuilder.Build();
        var childBuilder = new ContainerBuilder(parentContainer);
        var childContainer = childBuilder.Build();
        var expected = zeroParameterClass;
        var actual = childContainer.Resolve(typeof(ZeroParameterClass));
        Assert.Same(expected, actual);
    }
}
