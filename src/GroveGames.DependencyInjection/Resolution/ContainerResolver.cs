namespace GroveGames.DependencyInjection.Resolution;

internal sealed class ContainerResolver : IContainerResolver
{
    private readonly Dictionary<Type, IInstanceResolver> _instanceResolversByRegistrationTypes;
    private readonly IRegistrationResolver _parent;

    public ContainerResolver(IRegistrationResolver parent)
    {
        _instanceResolversByRegistrationTypes = [];
        _parent = parent;
    }

    public object Resolve(Type registrationType)
    {
        if (_instanceResolversByRegistrationTypes.TryGetValue(registrationType, out var instanceResolver))
        {
            return instanceResolver.Resolve();
        }

        return _parent.Resolve(registrationType);
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
