using System.Reflection;

namespace GroveGames.DependencyInjection.Activators;

internal sealed class MethodBaseActivator : IObjectActivator
{
    private readonly MethodBase _methodBase;
    private readonly Type[] _parameterTypes;

    public Type[] ParameterTypes => _parameterTypes;

    public MethodBaseActivator(MethodBase methodBase)
    {
        _methodBase = methodBase;
        _parameterTypes = Array.ConvertAll(methodBase.GetParameters(), (parameterInfo) => parameterInfo.ParameterType);
    }

    public void Activate(object uninitializedObject, params object[] parameters)
    {
        _methodBase.Invoke(uninitializedObject, parameters);
    }
}
