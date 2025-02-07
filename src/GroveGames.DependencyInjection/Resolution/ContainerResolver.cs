namespace GroveGames.DependencyInjection.Resolution;

public sealed class ContainerResolver : IContainerResolver
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
        return _instanceResolversByRegistrationTypes.TryGetValue(registrationType, out var instanceResolver)
            ? instanceResolver.Resolve()
            : _parent.Resolve(registrationType);
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