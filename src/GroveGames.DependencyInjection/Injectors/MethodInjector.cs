using System.Diagnostics.CodeAnalysis;
using System.Reflection;

using GroveGames.DependencyInjection.Activators;
using GroveGames.DependencyInjection.Resolution;

namespace GroveGames.DependencyInjection.Injectors;

internal static class MethodInjector
{
    private const BindingFlags MethodBindingFlags = BindingFlags.Public | BindingFlags.Instance;

    public static void Inject(object uninitializedObject, IObjectResolver resolver)
    {
        var implementationType = uninitializedObject.GetType();

        if (!TryCreateObjectActivator(implementationType, out var objectActivator))
        {
            return;
        }

        MethodBaseInjector.Inject(uninitializedObject, objectActivator!, resolver);
    }

    private static bool TryCreateObjectActivator([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)] Type implementationType, out IObjectActivator? activator)
    {
        activator = null;

        if (TryFindMethodInfo(implementationType, out var methodInfo))
        {
            activator = new MethodBaseActivator(methodInfo!);
        }

        return activator != null;
    }

    private static bool TryFindMethodInfo([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)] Type implementationType, out MethodInfo? foundMethodInfo)
    {
        foundMethodInfo = null;
        var methodInfos = implementationType.GetMethods(MethodBindingFlags);
        var foundParametersCount = int.MinValue;

        foreach (var methodInfo in methodInfos)
        {
            if (!methodInfo.IsDefined(typeof(InjectAttribute)))
            {
                continue;
            }

            var parametersCount = methodInfo.GetParameters().Length;

            if (foundParametersCount > parametersCount)
            {
                continue;
            }

            foundMethodInfo = methodInfo;
            foundParametersCount = parametersCount;
        }

        return foundMethodInfo != null;
    }
}