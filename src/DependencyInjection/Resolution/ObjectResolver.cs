using System.Runtime.CompilerServices;
using DependencyInjection.Collections;
using DependencyInjection.Injectors;

namespace DependencyInjection.Resolution;

internal sealed class ObjectResolver : IObjectResolver
{
    private readonly Type _implementationType;
    private readonly IRegistrationResolver _registrationResolver;
    private readonly IDisposableCollection _disposableCollection;

    public ObjectResolver(Type implementationType, IRegistrationResolver registrationResolver, IDisposableCollection disposableCollection)
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
