using System.Diagnostics.CodeAnalysis;

namespace GroveGames.DependencyInjection;

public interface IContainerBuilder
{
    public IContainerBuilder AddSingleton(Type serviceType, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type implementationType);
    public IContainerBuilder AddSingleton(Type serviceType, object instance);
    public IContainerBuilder AddSingleton(Type serviceType, Func<IObjectResolver, object> factory);
    public IContainerBuilder AddTransient(Type serviceType, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type implementationType);
    public IContainerBuilder AddTransient(Type serviceType, Func<IObjectResolver, object> factory);
}
