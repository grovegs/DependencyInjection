using GroveGames.DependencyInjection.Collections;
using GroveGames.DependencyInjection.Injectors;

namespace GroveGames.DependencyInjection.Resolution;

internal sealed class FactoryObjectResolver : IInstanceResolver
{
    private readonly Func<object> _factory;
    private readonly IObjectResolver _resolver;
    private readonly IDisposableCollection _disposables;

    public FactoryObjectResolver(Func<object> factory, IObjectResolver resolver, IDisposableCollection disposables)
    {
        _factory = factory;
        _resolver = resolver;
        _disposables = disposables;
    }

    public object Resolve()
    {
        var instance = _factory.Invoke();
        MethodInjector.Inject(instance, _resolver);
        _disposables.TryAdd(instance);
        return instance;
    }
}