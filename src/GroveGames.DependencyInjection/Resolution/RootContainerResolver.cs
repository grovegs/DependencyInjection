namespace GroveGames.DependencyInjection.Resolution;

internal sealed class RootContainerResolver : IContainerResolver
{
    private readonly Dictionary<Type, IInstanceResolver> _instanceResolversByRegistrationTypes;

    public RootContainerResolver()
    {
        _instanceResolversByRegistrationTypes = [];
    }

    public object Resolve(Type registrationType)
    {
        return _instanceResolversByRegistrationTypes.TryGetValue(registrationType, out var instanceResolver)
            ? instanceResolver.Resolve()
            : throw new RegistrationNotFoundException(registrationType);
    }

    public void AddInstanceResolver(Type registrationType, IInstanceResolver instanceResolver)
    {
        _instanceResolversByRegistrationTypes.Add(registrationType, instanceResolver);
    }

    public void Clear()
    {
        _instanceResolversByRegistrationTypes.Clear();
    }
}