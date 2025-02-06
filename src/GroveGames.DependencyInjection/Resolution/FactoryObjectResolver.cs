using GroveGames.DependencyInjection.Collections;
using GroveGames.DependencyInjection.Injectors;

namespace GroveGames.DependencyInjection.Resolution;

internal sealed class FactoryObjectResolver : IObjectResolver
{
    private readonly Func<object> _factory;
    private readonly IRegistrationResolver _registrationResolver;
    private readonly IDisposableCollection _disposables;

    public FactoryObjectResolver(Func<object> factory, IRegistrationResolver registrationResolver, IDisposableCollection disposables)
    {
        _factory = factory;
        _registrationResolver = registrationResolver;
        _disposables = disposables;
    }

    public object Resolve()
    {
        var instance = _factory.Invoke();
        MethodInjector.Inject(instance, _registrationResolver);
        _disposables.TryAdd(instance);
        return instance;
    }
}