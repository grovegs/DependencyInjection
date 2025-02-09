using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

using GroveGames.DependencyInjection.Collections;
using GroveGames.DependencyInjection.Injectors;

namespace GroveGames.DependencyInjection.Resolution;

internal sealed class UninitializedObjectResolver : IInstanceResolver
{
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors)]
    private readonly Type _implementationType;
    private readonly IObjectResolver _resolver;
    private readonly IDisposableCollection _disposables;

    public UninitializedObjectResolver([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors)] Type implementationType, IObjectResolver resolver, IDisposableCollection disposables)
    {
        _implementationType = implementationType;
        _resolver = resolver;
        _disposables = disposables;
    }

    public object Resolve()
    {
        var instance = RuntimeHelpers.GetUninitializedObject(_implementationType);
        ConstructorInjector.Inject(instance, _resolver);
        _disposables.TryAdd(instance);
        return instance;
    }
}