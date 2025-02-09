using GroveGames.DependencyInjection.Activators;
using GroveGames.DependencyInjection.Resolution;

namespace GroveGames.DependencyInjection.Injectors;

internal static class MethodBaseInjector
{
    public static void Inject(object uninitializedObject, IObjectActivator activator, IObjectResolver resolver)
    {
        var parameterTypes = activator.ParameterTypes;
        var parameterTypesLength = parameterTypes.Length;
        var parameters = new object[parameterTypesLength];

        for (var i = 0; i < parameterTypesLength; i++)
        {
            var parameterType = parameterTypes[i];
            parameters[i] = resolver.Resolve(parameterType);
        }

        activator.Activate(uninitializedObject, parameters);
    }
}