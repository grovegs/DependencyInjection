namespace GroveGames.DependencyInjection.Resolution;

public interface IContainerResolver : IRegistrationResolver
{
    void AddInstanceResolver(Type registrationType, IInstanceResolver instanceResolver);
    void Clear();
}