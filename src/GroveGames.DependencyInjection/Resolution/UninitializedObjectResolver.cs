using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

using GroveGames.DependencyInjection.Collections;
using GroveGames.DependencyInjection.Injectors;

namespace GroveGames.DependencyInjection.Resolution;

internal sealed class UninitializedObjectResolver : IObjectResolver
{
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors)]
    private readonly Type _implementationType;
    private readonly IRegistrationResolver _registrationResolver;
    private readonly IDisposableCollection _disposableCollection;

    public UninitializedObjectResolver([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors)] Type implementationType, IRegistrationResolver registrationResolver, IDisposableCollection disposableCollection)
    {
        _implementationType = implementationType;
        _registrationResolver = registrationResolver;
        _disposableCollection = disposableCollection;
    }

    public object Resolve()
    {
        var instance = RuntimeHelpers.GetUninitializedObject(_implementationType);
        ConstructorInjector.Inject(instance, _registrationResolver);
        _disposableCollection.TryAdd(instance);
        return instance;
    }
}