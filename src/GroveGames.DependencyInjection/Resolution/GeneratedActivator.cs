using GroveGames.DependencyInjection.Generated;

namespace GroveGames.DependencyInjection.Resolution;

internal sealed class GeneratedActivator : DependentActivator
{
    private readonly Func<object?[], object> _create;

    public GeneratedActivator(Type implementationType, GeneratedConstructor constructor)
        : base(implementationType, constructor.ParameterTypes)
    {
        _create = constructor.Create;
    }

    protected override object Instantiate(object?[] arguments)
    {
        return _create.Invoke(arguments);
    }
}
