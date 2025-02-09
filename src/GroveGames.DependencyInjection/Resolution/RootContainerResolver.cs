namespace GroveGames.DependencyInjection.Resolution;

internal sealed class RootContainerResolver : IContainerResolver
{
    private readonly Dictionary<Type, IInstanceResolver> _resolversByRegistrationTypes;

    public RootContainerResolver()
    {
        _resolversByRegistrationTypes = [];
    }

    public object Resolve(Type registrationType)
    {
        return _resolversByRegistrationTypes.TryGetValue(registrationType, out var resolver)
            ? resolver.Resolve()
            : throw new RegistrationNotFoundException(registrationType);
    }

    public void AddResolver(Type registrationType, IInstanceResolver resolver)
    {
        _resolversByRegistrationTypes.Add(registrationType, resolver);
    }

    public void Clear()
    {
        _resolversByRegistrationTypes.Clear();
    }
}