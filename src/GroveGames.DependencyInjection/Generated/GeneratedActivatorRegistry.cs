using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace GroveGames.DependencyInjection.Generated;

[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class GeneratedActivatorRegistry
{
    private readonly Dictionary<Type, GeneratedConstructor> _constructors;
    private readonly Dictionary<Type, List<GeneratedMethod>> _injectMethods;

    internal GeneratedActivatorRegistry()
    {
        _constructors = [];
        _injectMethods = [];
    }

    public void AddConstructor(Type type, Type[] parameterTypes, Func<object?[], object> create)
    {
        ArgumentNullException.ThrowIfNull(type);
        ArgumentNullException.ThrowIfNull(parameterTypes);
        ArgumentNullException.ThrowIfNull(create);
        _constructors[type] = new GeneratedConstructor(parameterTypes, create);
    }

    public void AddInjectMethod(Type type, Type[] parameterTypes, Action<object, object?[]> invoke)
    {
        ArgumentNullException.ThrowIfNull(type);
        ArgumentNullException.ThrowIfNull(parameterTypes);
        ArgumentNullException.ThrowIfNull(invoke);

        if (!_injectMethods.TryGetValue(type, out var methods))
        {
            methods = [];
            _injectMethods.Add(type, methods);
        }

        methods.Add(new GeneratedMethod(parameterTypes, invoke));
    }

    internal bool TryGetConstructor(Type type, [NotNullWhen(true)] out GeneratedConstructor? constructor)
    {
        return _constructors.TryGetValue(type, out constructor);
    }

    internal bool TryGetInjectMethods(Type type, [NotNullWhen(true)] out List<GeneratedMethod>? methods)
    {
        return _injectMethods.TryGetValue(type, out methods);
    }
}
