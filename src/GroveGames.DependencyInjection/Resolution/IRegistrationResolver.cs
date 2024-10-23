namespace GroveGames.DependencyInjection.Resolution;

public interface IRegistrationResolver
{
    internal object Resolve(Type registrationType);
}