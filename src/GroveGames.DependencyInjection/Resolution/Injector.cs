using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.ExceptionServices;

using GroveGames.DependencyInjection.Generated;

namespace GroveGames.DependencyInjection.Resolution;

internal sealed class Injector
{
    private readonly Type[][] _parameterTypes;
    private readonly Action<object, object?[]>[] _methods;

    public bool IsEmpty => _methods.Length == 0;

    private Injector(Type[][] parameterTypes, Action<object, object?[]>[] methods)
    {
        _parameterTypes = parameterTypes;
        _methods = methods;
    }

    public static Injector Create([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)] Type type)
    {
        if (GeneratedActivators.TryGetInjectMethods(type, out var generatedMethods))
        {
            return CreateGenerated(generatedMethods);
        }

        var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance);
        var injectMethods = new List<MethodInfo>();

        for (var i = 0; i < methods.Length; i++)
        {
            var method = methods[i];

            if (method.IsDefined(typeof(InjectAttribute), true))
            {
                injectMethods.Add(method);
            }
        }

        var parameterTypes = new Type[injectMethods.Count][];
        var invokers = new Action<object, object?[]>[injectMethods.Count];

        for (var i = 0; i < injectMethods.Count; i++)
        {
            var method = injectMethods[i];
            var parameters = method.GetParameters();
            var types = new Type[parameters.Length];

            for (var j = 0; j < parameters.Length; j++)
            {
                types[j] = parameters[j].ParameterType;
            }

            parameterTypes[i] = types;
            invokers[i] = (instance, arguments) => Invoke(method, instance, arguments);
        }

        return new Injector(parameterTypes, invokers);
    }

    public void Inject(object instance, IObjectResolver resolver)
    {
        for (var i = 0; i < _methods.Length; i++)
        {
            var parameterTypes = _parameterTypes[i];
            var arguments = new object?[parameterTypes.Length];

            for (var j = 0; j < parameterTypes.Length; j++)
            {
                arguments[j] = resolver.Resolve(parameterTypes[j]);
            }

            _methods[i].Invoke(instance, arguments);
        }
    }

    private static Injector CreateGenerated(List<GeneratedMethod> methods)
    {
        var parameterTypes = new Type[methods.Count][];
        var invokers = new Action<object, object?[]>[methods.Count];

        for (var i = 0; i < methods.Count; i++)
        {
            parameterTypes[i] = methods[i].ParameterTypes;
            invokers[i] = methods[i].Invoke;
        }

        return new Injector(parameterTypes, invokers);
    }

    private static void Invoke(MethodInfo method, object instance, object?[] arguments)
    {
        try
        {
            method.Invoke(instance, arguments);
        }
        catch (TargetInvocationException exception) when (exception.InnerException is not null)
        {
            ExceptionDispatchInfo.Capture(exception.InnerException).Throw();
            throw;
        }
    }
}
