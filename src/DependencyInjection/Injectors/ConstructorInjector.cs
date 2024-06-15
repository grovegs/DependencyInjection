using System.Reflection;
using DependencyInjection.Activators;
using DependencyInjection.Resolution;

namespace DependencyInjection.Injectors;

internal static class ConstructorInjector
{
    private const BindingFlags CostructorBindingFlags = BindingFlags.Public | BindingFlags.Instance;

    public static void Inject(object uninitializedObject, IRegistrationResolver registrationResolver)
    {
        var implementationType = uninitializedObject.GetType();

        if (!ObjectActivatorCache.TryGet(implementationType, out var objectActivator))
        {
            if (!TryCreateObjectActivator(implementationType, out objectActivator))
            {
                return;
            }

            ObjectActivatorCache.Add(implementationType, objectActivator);
        }

        MethodBaseInjector.Inject(uninitializedObject, objectActivator, registrationResolver);
    }

    private static bool TryCreateObjectActivator(Type implementationType, out IObjectActivator objectActivator)
    {
        objectActivator = null;

        if (TryFindConstructorInfo(implementationType, out var constructorInfo))
        {
            objectActivator = new MethodBaseActivator(constructorInfo);
        }

        return objectActivator != null;
    }

    private static bool TryFindConstructorInfo(Type implementationType, out ConstructorInfo foundConstructorInfo)
    {
        foundConstructorInfo = null;
        var constructorInfos = implementationType.GetConstructors(CostructorBindingFlags);
        var foundParametersCount = int.MinValue;

        foreach (var constructorInfo in constructorInfos)
        {
            var parametersCount = constructorInfo.GetParameters().Length;

            if (foundParametersCount > parametersCount) continue;

            foundConstructorInfo = constructorInfo;
            foundParametersCount = parametersCount;
        }

        return foundConstructorInfo != null;
    }
}
