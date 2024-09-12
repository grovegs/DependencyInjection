namespace DependencyInjection.Resolution;

public interface IRegistrationResolver
{
    internal object Resolve(Type registrationType);
}
