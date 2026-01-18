using System.Runtime.CompilerServices;

using GroveGames.DependencyInjection.Injectors;
using GroveGames.DependencyInjection.Resolution;

namespace GroveGames.DependencyInjection.Tests.Injectors;

public class ConstructorInjectorTests
{
    [Fact]
    public void Inject_ShouldCallRegistrationResolver_WhenConstructorIsFound()
    {
        var mockRegistrationResolver = new TestObjectResolver();
        var uninitializedObject = RuntimeHelpers.GetUninitializedObject(typeof(TestClassWithConstructor));
        mockRegistrationResolver.SetupResolve(typeof(string), "Test");

        ConstructorInjector.Inject(uninitializedObject, mockRegistrationResolver);

        Assert.Equal(1, mockRegistrationResolver.GetResolveCallCount(typeof(string)));
    }

    [Fact]
    public void Inject_ShouldNotCallRegistrationResolver_WhenNoPublicConstructorIsFound()
    {
        var mockRegistrationResolver = new TestObjectResolver();
        var uninitializedObject = RuntimeHelpers.GetUninitializedObject(typeof(TestClassWithoutPublicConstructor));

        ConstructorInjector.Inject(uninitializedObject, mockRegistrationResolver);

        Assert.Equal(0, mockRegistrationResolver.GetResolveCallCount(typeof(string)));
    }

    [Fact]
    public void Inject_ShouldUseConstructorWithMostParameters()
    {
        var mockRegistrationResolver = new TestObjectResolver();
        var uninitializedObject = RuntimeHelpers.GetUninitializedObject(typeof(TestClassWithConstructor));
        mockRegistrationResolver.SetupResolve(typeof(string), "Resolved Value");

        ConstructorInjector.Inject(uninitializedObject, mockRegistrationResolver);

        Assert.Equal(1, mockRegistrationResolver.GetResolveCallCount(typeof(string)));
    }

    private sealed class TestClassWithConstructor
    {
        public string? Name { get; }

        public TestClassWithConstructor(string name)
        {
            Name = name;
        }
    }

    private sealed class TestClassWithoutPublicConstructor
    {
        private TestClassWithoutPublicConstructor() { }
    }

    private sealed class TestObjectResolver : IObjectResolver
    {
        private readonly Dictionary<Type, object> _returnValues = new();
        private readonly Dictionary<Type, int> _resolveCallCounts = new();

        public void SetupResolve(Type type, object returnValue)
        {
            _returnValues[type] = returnValue;
        }

        public int GetResolveCallCount(Type type)
        {
            return _resolveCallCounts.TryGetValue(type, out var count) ? count : 0;
        }

        public object Resolve(Type registrationType)
        {
            _resolveCallCounts[registrationType] = GetResolveCallCount(registrationType) + 1;

            return _returnValues.TryGetValue(registrationType, out var value) ? value : null!;
        }
    }
}
