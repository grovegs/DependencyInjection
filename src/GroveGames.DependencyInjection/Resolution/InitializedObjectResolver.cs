using GroveGames.DependencyInjection.Collections;
using GroveGames.DependencyInjection.Injectors;

namespace GroveGames.DependencyInjection.Resolution;

internal sealed class InitializedObjectResolver : IInstanceResolver
{
    private readonly object _implementationInstance;
    private readonly IObjectResolver _resolver;
    private readonly IDisposableCollection _disposableCollection;

    public InitializedObjectResolver(object implementationInstance, IObjectResolver resolver, IDisposableCollection disposables)
    {
        _implementationInstance = implementationInstance;
        _resolver = resolver;
        _disposableCollection = disposables;
    }

    public object Resolve()
    {
        MethodInjector.Inject(_implementationInstance, _resolver);
        _disposableCollection.TryAdd(_implementationInstance);
        return _implementationInstance;
    }
}