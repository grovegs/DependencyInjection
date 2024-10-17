using GroveGames.DependencyInjection.Collections;
using GroveGames.DependencyInjection.Injectors;

namespace GroveGames.DependencyInjection.Resolution;

internal sealed class InitializedObjectResolver : IObjectResolver
{
    private readonly object _implementationInstance;
    private readonly IRegistrationResolver _registrationResolver;
    private readonly IDisposableCollection _disposableCollection;

    public InitializedObjectResolver(object implementationInstance, IRegistrationResolver registrationResolver, IDisposableCollection disposableCollection)
    {
        _implementationInstance = implementationInstance;
        _registrationResolver = registrationResolver;
        _disposableCollection = disposableCollection;
    }

    public object Resolve()
    {
        MethodInjector.Inject(_implementationInstance, _registrationResolver);
        _disposableCollection.TryAdd(_implementationInstance);
        return _implementationInstance;
    }
}
