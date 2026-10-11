namespace GroveGames.DependencyInjection.Resolution;

internal abstract class DependentActivator : InstanceActivator
{
    private readonly Type _implementationType;
    private readonly Type[] _parameterTypes;
    private readonly Binding[] _dependencies;
    private readonly object?[] _arguments;
    private bool _isActivating;

    public override Type ImplementationType => _implementationType;
    public override ReadOnlySpan<Binding> Dependencies => _dependencies;

    protected DependentActivator(Type implementationType, Type[] parameterTypes)
    {
        _implementationType = implementationType;
        _parameterTypes = parameterTypes;
        _dependencies = new Binding[parameterTypes.Length];
        _arguments = new object?[parameterTypes.Length];
    }

    public void Link(Container container)
    {
        for (var i = 0; i < _parameterTypes.Length; i++)
        {
            var parameterType = _parameterTypes[i];
            _dependencies[i] = container.FindBinding(parameterType) ?? throw new RegistrationNotFoundException(parameterType, _implementationType);
        }
    }

    public sealed override object Create()
    {
        if (_isActivating)
        {
            throw new CircularDependencyException(_implementationType);
        }

        _isActivating = true;

        try
        {
            for (var i = 0; i < _dependencies.Length; i++)
            {
                _arguments[i] = _dependencies[i].Resolve();
            }

            return Instantiate(_arguments);
        }
        finally
        {
            Array.Clear(_arguments, 0, _arguments.Length);
            _isActivating = false;
        }
    }

    protected abstract object Instantiate(object?[] arguments);
}
