namespace GroveGames.DependencyInjection.Resolution;

internal sealed class SingletonResolver : IInstanceResolver
{
    private readonly IInstanceResolver _resolver;
    private object? _implementationInstance;

    public SingletonResolver(IInstanceResolver resolver)
    {
        _resolver = resolver;
    }

    public object Resolve()
    {
        _implementationInstance ??= _resolver.Resolve();

        return _implementationInstance;
    }
}