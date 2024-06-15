using System.Reflection;
using DependencyInjection.Activators;
using DependencyInjection.Core;
using DependencyInjection.Resolution;

namespace DependencyInjection.Injectors;

internal static class MethodInjector
{
    private const BindingFlags MethodBindingFlags = BindingFlags.Public | BindingFlags.Instance;

    public static void Inject(object uninitializedObject, IRegistrationResolver registrationResolver)
    {
        var implementationType = uninitializedObject.GetType();

        if (!ObjectActivatorCache.TryGet(implementationType, out var objectActivator))
        {
            if (!TryCreateObjectActivator(implementationType, out objectActivator)) return;

            ObjectActivatorCache.Add(implementationType, objectActivator);
        }

        MethodBaseInjector.Inject(uninitializedObject, objectActivator, registrationResolver);
    }

    private static bool TryCreateObjectActivator(Type implementationType, out IObjectActivator objectActivator)
    {
        objectActivator = null;

        if (TryFindMethodInfo(implementationType, out var constructorInfo))
        {
            objectActivator = new MethodBaseActivator(constructorInfo);
        }

        return objectActivator != null;
    }

    private static bool TryFindMethodInfo(Type implementationType, out MethodInfo foundMethodInfo)
    {
        foundMethodInfo = null;
        var methodInfos = implementationType.GetMethods(MethodBindingFlags);
        var foundParametersCount = int.MinValue;

        foreach (var methodInfo in methodInfos)
        {
            if (!methodInfo.IsDefined(typeof(InjectAttribute))) continue;

            var parametersCount = methodInfo.GetParameters().Length;

            if (foundParametersCount > parametersCount) continue;

            foundMethodInfo = methodInfo;
            foundParametersCount = parametersCount;
        }

        return foundMethodInfo != null;
    }
}
