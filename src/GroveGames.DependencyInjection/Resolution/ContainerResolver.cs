namespace GroveGames.DependencyInjection.Resolution;

public sealed class ContainerResolver : IContainerResolver
{
    private readonly Dictionary<Type, IInstanceResolver> _resolversByRegistrationTypes;
    private readonly IObjectResolver _parent;

    public ContainerResolver(IObjectResolver parent)
    {
        _resolversByRegistrationTypes = [];
        _parent = parent;
    }

    public object Resolve(Type registrationType)
    {
        return _resolversByRegistrationTypes.TryGetValue(registrationType, out var resolver)
            ? resolver.Resolve()
            : _parent.Resolve(registrationType);
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