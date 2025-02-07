namespace GroveGames.DependencyInjection.Resolution;

public interface IContainerResolver : IObjectResolver
{
    void AddInstanceResolver(Type registrationType, IInstanceResolver resolver);
    void Clear();
}