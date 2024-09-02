using DependencyInjection.Core;
using DependencyInjection.Resolution;
using DependencyInjection.Tests.Fakes;

namespace DependencyInjection.Tests;

public sealed class ContainerResolverTests
{
    [Fact]
    public void Resolve_InstanceWithZeroParameterClassInstanceWithRegistrationType_ShouldReturnInstanceOfRegistrationType()
    {
        var containerResolver = new ContainerResolver(null);
        var instanceResolver = new InstanceResolver(new ZeroParameterClass());
        containerResolver.AddInstanceResolver(typeof(IZeroParameterClass), instanceResolver);
        var instance = containerResolver.Resolve(typeof(IZeroParameterClass));
        Assert.IsType<ZeroParameterClass>(instance);
    }

    [Fact]
    public void Resolve_InstanceWithZeroParameterClassInstanceWithImplementationType_ShouldReturnInstanceOfImplementationType()
    {
        var containerResolver = new ContainerResolver(null);
        var instanceResolver = new InstanceResolver(new ZeroParameterClass());
        containerResolver.AddInstanceResolver(typeof(ZeroParameterClass), instanceResolver);
        var instance = containerResolver.Resolve(typeof(ZeroParameterClass));
        Assert.IsType<ZeroParameterClass>(instance);
    }

    [Fact]
    public void Resolve_SingletonMultipleTimes_ShouldReturnSameInstances()
    {
        var containerResolver = new ContainerResolver(null);
        var disposableCollection = new Mock<IDisposableCollection>();
        var objectResolver = new ObjectResolver(typeof(ZeroParameterClass), containerResolver, disposableCollection.Object);
        var instanceResolver = new SingletonResolver(objectResolver);
        containerResolver.AddInstanceResolver(typeof(IZeroParameterClass), instanceResolver);
        var instance1 = containerResolver.Resolve(typeof(IZeroParameterClass));
        var instance2 = containerResolver.Resolve(typeof(IZeroParameterClass));
        Assert.Same(instance1, instance2);
    }

    [Fact]
    public void Resolve_SingletonWithZeroParameterClassWithRegistrationType_ShouldReturnInstanceOfRegistrationType()
    {
        var containerResolver = new ContainerResolver(null);
        var disposableCollection = new Mock<IDisposableCollection>();
        var objectResolver = new ObjectResolver(typeof(ZeroParameterClass), containerResolver, disposableCollection.Object);
        var instanceResolver = new SingletonResolver(objectResolver);
        containerResolver.AddInstanceResolver(typeof(IZeroParameterClass), instanceResolver);
        var instance = containerResolver.Resolve(typeof(IZeroParameterClass));
        Assert.IsAssignableFrom<IZeroParameterClass>(instance);
    }

    [Fact]
    public void Resolve_SingletonWithZeroParameterClassWithImplementationType_ShouldReturnInstanceOfImplementationType()
    {
        var containerResolver = new ContainerResolver(null);
        var disposableCollection = new Mock<IDisposableCollection>();
        var objectResolver = new ObjectResolver(typeof(ZeroParameterClass), containerResolver, disposableCollection.Object);
        var instanceResolver = new SingletonResolver(objectResolver);
        containerResolver.AddInstanceResolver(typeof(ZeroParameterClass), instanceResolver);
        var instance = containerResolver.Resolve(typeof(ZeroParameterClass));
        Assert.IsType<ZeroParameterClass>(instance);
    }

    [Fact]
    public void Resolve_SingletonWithOneParameterClassWithRegistrationType_ShouldParameterReturnInstanceOfParameterType()
    {
        var containerResolver = new ContainerResolver(null);
        var disposableCollection = new Mock<IDisposableCollection>();
        var zeroParameterClassResolver = new ObjectResolver(typeof(ZeroParameterClass), containerResolver, disposableCollection.Object);
        var oneParameterClassResolver = new ObjectResolver(typeof(OneParameterClass), containerResolver, disposableCollection.Object);
        containerResolver.AddInstanceResolver(typeof(IZeroParameterClass), new SingletonResolver(zeroParameterClassResolver));
        containerResolver.AddInstanceResolver(typeof(IOneParameterClass), new SingletonResolver(oneParameterClassResolver));
        var instance = containerResolver.Resolve(typeof(IOneParameterClass)) as OneParameterClass;
        var parameterInstance = instance?.GetZeroParameterClass();
        Assert.IsAssignableFrom<IZeroParameterClass>(parameterInstance);
    }

    [Fact]
    public void Resolve_TransientMultipleTimes_ShouldReturnDifferentInstances()
    {
        var containerResolver = new ContainerResolver(null);
        var disposableCollection = new Mock<IDisposableCollection>();
        var zeroParameterClassResolver = new ObjectResolver(typeof(ZeroParameterClass), containerResolver, disposableCollection.Object);
        containerResolver.AddInstanceResolver(typeof(IZeroParameterClass), new TransientResolver(zeroParameterClassResolver));
        var instance1 = containerResolver.Resolve(typeof(IZeroParameterClass));
        var instance2 = containerResolver.Resolve(typeof(IZeroParameterClass));
        Assert.NotSame(instance1, instance2);
    }
}
