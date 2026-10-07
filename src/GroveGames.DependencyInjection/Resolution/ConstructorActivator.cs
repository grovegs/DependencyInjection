using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.ExceptionServices;

namespace GroveGames.DependencyInjection.Resolution;

internal sealed class ConstructorActivator : InstanceActivator
{
    private readonly Type _implementationType;
    private readonly ConstructorInfo _constructor;
    private readonly Type[] _parameterTypes;
    private readonly Binding[] _dependencies;
    private readonly object?[] _arguments;
    private bool _isActivating;

    public override Type ImplementationType => _implementationType;
    public override ReadOnlySpan<Binding> Dependencies => _dependencies;

    public ConstructorActivator([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type implementationType)
    {
        _implementationType = implementationType;
        _constructor = FindConstructor(implementationType);
        var parameters = _constructor.GetParameters();
        _parameterTypes = new Type[parameters.Length];

        for (var i = 0; i < parameters.Length; i++)
        {
            _parameterTypes[i] = parameters[i].ParameterType;
        }

        _dependencies = new Binding[parameters.Length];
        _arguments = new object?[parameters.Length];
    }

    public void Link(Container container)
    {
        for (var i = 0; i < _parameterTypes.Length; i++)
        {
            var parameterType = _parameterTypes[i];
            _dependencies[i] = container.FindBinding(parameterType) ?? throw new RegistrationNotFoundException(parameterType, _implementationType);
        }
    }

    public override object Create()
    {
        if (_isActivating)
        {
            throw new CircularDependencyException(_implementationType);
        }

        _isActivating = true;

        try
        {
            for (var i = 0; i < _dependencies.Length; i++)
            {
                _arguments[i] = _dependencies[i].Resolve();
            }

            return _constructor.Invoke(_arguments);
        }
        catch (TargetInvocationException exception) when (exception.InnerException is not null)
        {
            ExceptionDispatchInfo.Capture(exception.InnerException).Throw();
            throw;
        }
        finally
        {
            Array.Clear(_arguments, 0, _arguments.Length);
            _isActivating = false;
        }
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
