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
        if (_instanceResolversByRegistrationTypes.TryGetValue(registrationType, out var instanceResolver))
        {
            return instanceResolver.Resolve();
        }

        throw new InvalidOperationException($"No registration found for type {registrationType}.");
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