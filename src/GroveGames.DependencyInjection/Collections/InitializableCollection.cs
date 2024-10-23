using GroveGames.DependencyInjection.Resolution;

namespace GroveGames.DependencyInjection.Collections;

internal sealed class InitializableCollection : IInitializableCollection
{
    private static readonly Type s_initializableType = typeof(IInitializable);

    private readonly List<Type> _initializableRegistrationTypes;
    private readonly IRegistrationResolver _registrationResolver;

    public InitializableCollection(IRegistrationResolver registrationResolver)
    {
        _initializableRegistrationTypes = [];
        _registrationResolver = registrationResolver;
    }

    public void TryAdd(Type registrationType, Type implementationType)
    {
        // TODO: Very costly operation. Find better way.
        if (s_initializableType.IsAssignableFrom(implementationType))
        {
            _initializableRegistrationTypes.Add(registrationType);
        }
    }

    public void Initialize()
    {
        foreach (var initializableRegistrationType in _initializableRegistrationTypes)
        {
            var initializable = _registrationResolver.Resolve(initializableRegistrationType) as IInitializable;
            initializable?.Initialize();
        }
    }
}