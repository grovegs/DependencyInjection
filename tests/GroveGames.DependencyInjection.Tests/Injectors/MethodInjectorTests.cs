using GroveGames.DependencyInjection.Injectors;
using GroveGames.DependencyInjection.Resolution;

namespace GroveGames.DependencyInjection.Tests.Injectors;

public class MethodInjectorTests
{
    [Fact]
    public void Inject_ShouldCallMethodBaseInjector_WhenInjectMethodIsFound()
    {
        var mockRegistrationResolver = new TestObjectResolver();
        var testObject = new TestClassWithInjectMethod();
        mockRegistrationResolver.SetupResolve(typeof(string), "ResolvedValue");

        MethodInjector.Inject(testObject, mockRegistrationResolver);

        Assert.True(testObject.MethodCalled);
        Assert.Equal("ResolvedValue", testObject.StringValue);
        Assert.Equal(1, mockRegistrationResolver.GetResolveCallCount(typeof(string)));
    }

    [Fact]
    public void Inject_ShouldNotCallMethodBaseInjector_WhenNoInjectMethodIsFound()
    {
        var mockRegistrationResolver = new TestObjectResolver();
        var testObject = new TestClassWithoutInjectMethod();

        MethodInjector.Inject(testObject, mockRegistrationResolver);

        Assert.Equal(0, mockRegistrationResolver.GetResolveCallCount(typeof(string)));
    }

    [Fact]
    public void Inject_ShouldSelectMethodWithMostParameters_WhenMultipleInjectMethodsExist()
    {
        var testObject = new TestClassWithMultipleInjectMethods();
        var mockRegistrationResolver = new TestObjectResolver();
        mockRegistrationResolver.SetupResolve(typeof(string), "StringValue");
        mockRegistrationResolver.SetupResolve(typeof(int), 42);

        MethodInjector.Inject(testObject, mockRegistrationResolver);

        Assert.Equal("StringValue", testObject.StringValue);
        Assert.Equal(42, testObject.IntValue);
        Assert.Equal(1, mockRegistrationResolver.GetResolveCallCount(typeof(string)));
        Assert.Equal(1, mockRegistrationResolver.GetResolveCallCount(typeof(int)));
    }

    private sealed class TestClassWithInjectMethod
    {
        public bool MethodCalled { get; private set; } = false;
        public string? StringValue { get; private set; }

        [Inject]
        public void Initialize(string value)
        {
            MethodCalled = true;
            StringValue = value;
        }
    }


    private sealed class TestClassWithMultipleInjectMethods
    {
        public string? StringValue { get; private set; }
        public int IntValue { get; private set; }

        [Inject]
        public void InjectMethod1(string value)
        {
            StringValue = value;
        }

        [Inject]
        public void InjectMethod2(string value, int number)
        {
            StringValue = value;
            IntValue = number;
        }
    }

    private sealed class TestClassWithoutInjectMethod
    {
        public void RegularMethod() { }
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
