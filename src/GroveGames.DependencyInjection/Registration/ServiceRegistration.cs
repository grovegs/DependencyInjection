using System.Diagnostics.CodeAnalysis;

namespace GroveGames.DependencyInjection.Registration;

internal sealed class ServiceRegistration
{
    private readonly Type _serviceType;
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
    private readonly Type _implementationType;
    private readonly Lifetime _lifetime;
    private readonly object? _instance;
    private readonly Func<IObjectResolver, object>? _factory;

    public Type ServiceType => _serviceType;
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
    public Type ImplementationType => _implementationType;
    public Lifetime Lifetime => _lifetime;
    public object? Instance => _instance;
    public Func<IObjectResolver, object>? Factory => _factory;

    public ServiceRegistration(Type serviceType, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type implementationType, Lifetime lifetime, object? instance, Func<IObjectResolver, object>? factory)
    {
        _serviceType = serviceType;
        _implementationType = implementationType;
        _lifetime = lifetime;
        _instance = instance;
        _factory = factory;
    }
}
