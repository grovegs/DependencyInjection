namespace GroveGames.DependencyInjection.Resolution;

public interface IRegistrationResolver
{
    object Resolve(Type registrationType);
}