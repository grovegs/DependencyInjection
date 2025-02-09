using System.Diagnostics.CodeAnalysis;

namespace GroveGames.DependencyInjection;

public interface IContainerBuilder
{
    IContainerBuilder AddSingleton(Type registrationType, object implementationInstance);

    IContainerBuilder AddSingleton([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors)] Type registrationType, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors)] Type implementationType);
    IContainerBuilder AddSingleton(Type registrationType, Func<object> instanceFactory);
    IContainerBuilder AddTransient([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors)] Type registrationType, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors)] Type implementationType);
    IContainerBuilder AddTransient(Type registrationType, Func<object> instanceFactory);
}