namespace DependencyInjection.Core;

public interface IContainerConfigurer
{
    void AddInstance(Type registrationType, object implementationInstance);
    void AddSingleton(Type registrationType, Type implementationType);
    void AddTransient(Type registrationType, Type implementationType);
}