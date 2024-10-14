namespace GroveGames.DependencyInjection.Resolution;

internal sealed class SingletonResolver : IInstanceResolver
{
    private readonly IObjectResolver _objectResolver;
    private object? _implementationInstance;

    public SingletonResolver(IObjectResolver objectResolver)
    {
        _objectResolver = objectResolver;
    }

    public object Resolve()
    {
        _implementationInstance ??= _objectResolver.Resolve();

        return _implementationInstance;
    }
}
