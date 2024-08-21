using DependencyInjection.Core;
using DependencyInjection.Injectors;
using DependencyInjection.Resolution;
using DependencyInjection.Tests.Fakes;

namespace DependencyInjection.Tests;

public sealed class MethodInjectorTests
{
    [Fact]
    public void Inject_ClassWithInjectableMethod_ShouldReturnSameInstanceOfParameter()
    {
        var containerResolver = new ContainerResolver(Containers.Root);
        var zeroParameterClass = new ZeroParameterClass();
        var objectResolver = new InstanceResolver(zeroParameterClass);
        containerResolver.AddInstanceResolver(typeof(IZeroParameterClass), objectResolver);
        var classWithInjectableMethod = new ClassWithInjectableMethod();
        MethodInjector.Inject(classWithInjectableMethod, containerResolver);
        var actual = classWithInjectableMethod.GetZeroParameterClass();
        var expected = zeroParameterClass;
        Assert.Equal(expected, actual);
    }
}
