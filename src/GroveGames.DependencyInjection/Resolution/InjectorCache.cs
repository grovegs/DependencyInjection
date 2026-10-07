using System.Diagnostics.CodeAnalysis;

namespace GroveGames.DependencyInjection.Resolution;

internal sealed class InjectorCache
{
    private readonly Dictionary<Type, Injector> _injectors;

    public InjectorCache()
    {
        _injectors = [];
    }

    [UnconditionalSuppressMessage("Trimming", "IL2067", Justification = "Types with [Inject] methods must be preserved by the caller.")]
    public Injector Get(Type type)
    {
        if (!_injectors.TryGetValue(type, out var injector))
        {
            injector = Injector.Create(type);
            _injectors.Add(type, injector);
        }

        return injector;
    }
}
