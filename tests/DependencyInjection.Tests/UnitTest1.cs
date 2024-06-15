using System.Runtime.CompilerServices;
using DependencyInjection.Core;
using DependencyInjection.Injectors;
using DependencyInjection.Resolution;
using DependencyInjection.Tests.Fakes;

namespace DependencyInjection.Tests;

public class UnitTest1
{
    [Fact]
    public void Inject_OneParameterClassWithConstructor_ShouldReturnSameInstanceOfParameter()
    {
        var containerResolver = new ContainerResolver(Container.Root);
        var zeroParameterClass = new ZeroParameterClass();
        var objectResolver = new InstanceResolver(zeroParameterClass);
        containerResolver.AddInstanceResolver(typeof(IZeroParameterClass), objectResolver);
        var oneParameterClass = (OneParameterClass)RuntimeHelpers.GetUninitializedObject(typeof(OneParameterClass));
        ConstructorInjector.Inject(oneParameterClass, containerResolver);
        var actual = oneParameterClass.GetZeroParameterClass();
        var expected = zeroParameterClass;
        Assert.Equal(expected, actual);
    }
}