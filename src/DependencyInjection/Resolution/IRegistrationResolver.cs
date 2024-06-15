namespace DependencyInjection.Resolution;

public interface IRegistrationResolver
{
    object Resolve(Type registrationType);
}