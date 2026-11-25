namespace GroveGames.DependencyInjection.Resolution;

internal sealed class FactoryActivator : InstanceActivator
{
    private readonly Type _implementationType;
    private readonly Func<IObjectResolver, object> _factory;
    private bool _isActivating;
    private bool _isValidated;

    public override Type ImplementationType => _implementationType;

    public FactoryActivator(Type implementationType, Func<IObjectResolver, object> factory)
    {
        _implementationType = implementationType;
        _factory = factory;
    }

    public override object Create(Container requester)
    {
        if (_isActivating)
        {
            throw new CircularDependencyException(_implementationType);
        }

        _isActivating = true;

        try
        {
            var instance = _factory.Invoke(requester);

            if (instance is null)
            {
                throw new InvalidOperationException($"Factory for {_implementationType} returned null.");
            }

            if (!_isValidated)
            {
                if (!_implementationType.IsInstanceOfType(instance))
                {
                    throw new InvalidOperationException($"Factory for {_implementationType} returned {instance.GetType()}.");
                }

                _isValidated = true;
            }

            return instance;
        }
        finally
        {
            _isActivating = false;
        }
    }
}
