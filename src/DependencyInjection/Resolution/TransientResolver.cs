namespace DependencyInjection.Resolution;

internal sealed class TransientResolver : IInstanceResolver
{
    private readonly IObjectResolver _objectResolver;

    public TransientResolver(IObjectResolver objectResolver)
    {
        _objectResolver = objectResolver;
    }

    public object Resolve()
    {
        return _objectResolver.Resolve();
    }
}
