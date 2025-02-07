namespace GroveGames.DependencyInjection.Resolution;

public interface IContainerResolver : IObjectResolver
{
    void AddResolver(Type registrationType, IInstanceResolver resolver);
    void Clear();
}