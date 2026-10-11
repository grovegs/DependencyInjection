namespace GroveGames.DependencyInjection.Generated;

internal sealed class GeneratedConstructor
{
    private readonly Type[] _parameterTypes;
    private readonly Func<object?[], object> _create;

    public Type[] ParameterTypes => _parameterTypes;
    public Func<object?[], object> Create => _create;

    public GeneratedConstructor(Type[] parameterTypes, Func<object?[], object> create)
    {
        _parameterTypes = parameterTypes;
        _create = create;
    }
}
