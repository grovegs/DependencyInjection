using DependencyInjection.Activators;

namespace DependencyInjection.Tests.Mocks;

internal sealed class MockObjectActivator : IObjectActivator
{
    private readonly Type[] _parameterTypes;
    private readonly Action<object, object[]> _onActivate;

    public Type[] ParameterTypes => _parameterTypes;

    public MockObjectActivator(Type[] parameterTypes = null, Action<object, object[]> onActivate = null)
    {
        _parameterTypes = parameterTypes;
        _onActivate = onActivate;
    }

    public void Activate(object uninitializedObject, params object[] parameters)
    {
        _onActivate?.Invoke(uninitializedObject, parameters);
    }
}
