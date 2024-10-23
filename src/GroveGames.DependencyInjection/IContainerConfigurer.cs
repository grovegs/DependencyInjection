using GroveGames.DependencyInjection.Resolution;

namespace GroveGames.DependencyInjection;

public interface IContainerConfigurer
{
    void AddSingleton(Type registrationType, object implementationInstance);
    void AddSingleton(Type registrationType, Type implementationType);
    void AddTransient(Type registrationType, Type implementationType);
}