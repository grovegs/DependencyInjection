namespace GroveGames.DependencyInjection.Generated;

internal sealed class GeneratedMethod
{
    private readonly Type[] _parameterTypes;
    private readonly Action<object, object?[]> _invoke;

    public Type[] ParameterTypes => _parameterTypes;
    public Action<object, object?[]> Invoke => _invoke;

    public GeneratedMethod(Type[] parameterTypes, Action<object, object?[]> invoke)
    {
        _parameterTypes = parameterTypes;
        _invoke = invoke;
    }
}
