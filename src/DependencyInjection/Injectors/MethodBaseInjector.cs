using DependencyInjection.Activators;
using DependencyInjection.Resolution;

namespace DependencyInjection.Injectors;

internal static class MethodBaseInjector
{
    public static void Inject(object uninitializedObject, IObjectActivator objectActivator, IRegistrationResolver registrationResolver)
    {
        var parameterTypes = objectActivator.ParameterTypes;
        var parameterTypesLength = parameterTypes.Length;
        var parameters = new object[parameterTypesLength];

        for (var i = 0; i < parameterTypesLength; i++)
        {
            var parameterType = parameterTypes[i];
            parameters[i] = registrationResolver.Resolve(parameterType);
        }

        objectActivator.Activate(uninitializedObject, parameters);
    }
}
