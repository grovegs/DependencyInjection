using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.ExceptionServices;

namespace GroveGames.DependencyInjection.Resolution;

internal sealed class Injector
{
    private readonly MethodInfo[] _methods;
    private readonly Type[][] _parameterTypes;

    public bool IsEmpty => _methods.Length == 0;

    private Injector(MethodInfo[] methods, Type[][] parameterTypes)
    {
        _methods = methods;
        _parameterTypes = parameterTypes;
    }

    public static Injector Create([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)] Type type)
    {
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

        for (var i = 0; i < injectMethods.Count; i++)
        {
            var parameters = injectMethods[i].GetParameters();
            var types = new Type[parameters.Length];

            for (var j = 0; j < parameters.Length; j++)
            {
                types[j] = parameters[j].ParameterType;
            }

            parameterTypes[i] = types;
        }

        return new Injector(injectMethods.ToArray(), parameterTypes);
    }

    public void Inject(object instance, IObjectResolver resolver)
    {
        for (var i = 0; i < _methods.Length; i++)
        {
            var parameterTypes = _parameterTypes[i];
            var arguments = new object[parameterTypes.Length];

            for (var j = 0; j < parameterTypes.Length; j++)
            {
                arguments[j] = resolver.Resolve(parameterTypes[j]);
            }

            try
            {
                _methods[i].Invoke(instance, arguments);
            }
            catch (TargetInvocationException exception) when (exception.InnerException is not null)
            {
                ExceptionDispatchInfo.Capture(exception.InnerException).Throw();
                throw;
            }
        }
    }
}
