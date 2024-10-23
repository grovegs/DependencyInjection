namespace GroveGames.DependencyInjection.Resolution;

internal sealed class InstanceResolver : IInstanceResolver
{
    private readonly IObjectResolver _objectResolver;
    private object? _implementationInstance;

    public InstanceResolver(IObjectResolver objectResolver)
    {
        _objectResolver = objectResolver;
    }

    public object Resolve()
    {
        _implementationInstance ??= _objectResolver.Resolve();

        return _implementationInstance;
    }
}