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

        if (!TryCreateObjectActivator(implementationType, out var objectActivator))
        {
            return;
        }

        MethodBaseInjector.Inject(uninitializedObject, objectActivator!, registrationResolver);
    }

    private static bool TryCreateObjectActivator(Type implementationType, out IObjectActivator? objectActivator)
    {
        if (TryFindConstructorInfo(implementationType, out var constructorInfo))
        {
            objectActivator = new MethodBaseActivator(constructorInfo!);
            return true;
        }

        objectActivator = null;
        return false;
    }

    private static bool TryFindConstructorInfo(Type implementationType, out ConstructorInfo? foundConstructorInfo)
    {
        var constructorInfos = implementationType.GetConstructors(CostructorBindingFlags);
        var foundParametersCount = int.MinValue;

        foreach (var constructorInfo in constructorInfos)
        {
            var parametersCount = constructorInfo.GetParameters().Length;

            if (foundParametersCount > parametersCount) continue;

            foundConstructorInfo = constructorInfo;
            foundParametersCount = parametersCount;
            return true;
        }

        foundConstructorInfo = null;
        return false;
    }
}
