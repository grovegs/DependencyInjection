namespace GroveGames.DependencyInjection.Resolution;

internal sealed class TransientResolver : IInstanceResolver
{
    private readonly IInstanceResolver _resolver;

    public TransientResolver(IInstanceResolver resolver)
    {
        _resolver = resolver;
    }

    public object Resolve()
    {
        return _resolver.Resolve();
    }
}