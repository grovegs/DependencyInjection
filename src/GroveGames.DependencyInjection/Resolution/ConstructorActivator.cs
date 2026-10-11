using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.ExceptionServices;

namespace GroveGames.DependencyInjection.Resolution;

internal sealed class ConstructorActivator : DependentActivator
{
    private readonly ConstructorInfo _constructor;

    public ConstructorActivator([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type implementationType)
        : this(implementationType, FindConstructor(implementationType))
    {
    }

    private ConstructorActivator(Type implementationType, ConstructorInfo constructor)
        : base(implementationType, GetParameterTypes(constructor))
    {
        _constructor = constructor;
    }

    protected override object Instantiate(object?[] arguments)
    {
        try
        {
            return _constructor.Invoke(arguments);
        }
        catch (TargetInvocationException exception) when (exception.InnerException is not null)
        {
            ExceptionDispatchInfo.Capture(exception.InnerException).Throw();
            throw;
        }
    }

    private static Type[] GetParameterTypes(ConstructorInfo constructor)
    {
        var parameters = constructor.GetParameters();
        var parameterTypes = new Type[parameters.Length];

        for (var i = 0; i < parameters.Length; i++)
        {
            parameterTypes[i] = parameters[i].ParameterType;
        }

        return parameterTypes;
    }

    private static ConstructorInfo FindConstructor([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type implementationType)
    {
        if (implementationType.IsAbstract || implementationType.IsInterface)
        {
            throw new ArgumentException($"{implementationType} must be a concrete type.", nameof(implementationType));
        }

        var constructors = implementationType.GetConstructors(BindingFlags.Public | BindingFlags.Instance);
        ConstructorInfo? selected = null;
        var selectedParameterCount = -1;

        for (var i = 0; i < constructors.Length; i++)
        {
            var constructor = constructors[i];

            if (constructor.IsDefined(typeof(InjectAttribute), false))
            {
                return constructor;
            }

            var parameterCount = constructor.GetParameters().Length;

            if (parameterCount > selectedParameterCount)
            {
                selected = constructor;
                selectedParameterCount = parameterCount;
            }
        }

        return selected ?? throw new ArgumentException($"{implementationType} has no public constructor.", nameof(implementationType));
    }
}
