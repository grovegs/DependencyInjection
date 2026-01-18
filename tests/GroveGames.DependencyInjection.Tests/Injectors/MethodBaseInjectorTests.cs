using GroveGames.DependencyInjection.Activators;
using GroveGames.DependencyInjection.Injectors;
using GroveGames.DependencyInjection.Resolution;

namespace GroveGames.DependencyInjection.Tests.Injectors;

public class MethodBaseInjectorTests
{
    [Fact]
    public void Inject_ShouldResolveAllParametersAndActivateObject()
    {
        var mockObjectActivator = new TestObjectActivator();
        var mockRegistrationResolver = new TestObjectResolver();

        var uninitializedObject = new object();
        var parameterTypes = new[] { typeof(string), typeof(int) };
        mockObjectActivator.ParameterTypes = parameterTypes;

        mockRegistrationResolver.SetupResolve(typeof(string), "test-string");
        mockRegistrationResolver.SetupResolve(typeof(int), 42);

        MethodBaseInjector.Inject(uninitializedObject, mockObjectActivator, mockRegistrationResolver);

        Assert.Equal(1, mockRegistrationResolver.GetResolveCallCount(typeof(string)));
        Assert.Equal(1, mockRegistrationResolver.GetResolveCallCount(typeof(int)));

        Assert.Single(mockObjectActivator.ActivateCalls);
        var activateCall = mockObjectActivator.ActivateCalls[0];
        Assert.Equal(uninitializedObject, activateCall.Target);
        Assert.Equal(2, activateCall.Parameters.Length);
        Assert.Equal("test-string", activateCall.Parameters[0]);
        Assert.Equal(42, activateCall.Parameters[1]);
    }

    private sealed class TestObjectActivator : IObjectActivator
    {
        private readonly List<(object Target, object[] Parameters)> _activateCalls = new();

        public Type[] ParameterTypes { get; set; } = Array.Empty<Type>();
        public IReadOnlyList<(object Target, object[] Parameters)> ActivateCalls => _activateCalls;

        public void Activate(object uninitializedObject, params object[] parameters)
        {
            _activateCalls.Add((uninitializedObject, parameters));
        }
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
