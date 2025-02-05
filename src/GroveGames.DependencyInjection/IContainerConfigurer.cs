using System.Diagnostics.CodeAnalysis;

namespace GroveGames.DependencyInjection;

public interface IContainerConfigurer
{
    void AddSingleton(Type registrationType, object implementationInstance);

    void AddSingleton([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors)] Type registrationType, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors)] Type implementationType);
    void AddSingleton(Type registrationType, Func<object> factory);
    void AddTransient([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors)] Type registrationType, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors)] Type implementationType);
    void AddTransient(Type registrationType, Func<object> factory);
}