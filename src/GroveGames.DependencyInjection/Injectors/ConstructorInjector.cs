using System.Diagnostics.CodeAnalysis;
using System.Reflection;

using GroveGames.DependencyInjection.Activators;
using GroveGames.DependencyInjection.Resolution;

namespace GroveGames.DependencyInjection.Injectors;

internal static class ConstructorInjector
{
    private const BindingFlags ConstructorBindingFlags = BindingFlags.Public | BindingFlags.Instance;

    public static void Inject(object uninitializedObject, IRegistrationResolver registrationResolver)
    {
        var implementationType = uninitializedObject.GetType();

        if (!TryCreateObjectActivator(implementationType, out var objectActivator))
        {
            return;
        }

        MethodBaseInjector.Inject(uninitializedObject, objectActivator!, registrationResolver);
    }

    private static bool TryCreateObjectActivator([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type implementationType, out IObjectActivator? objectActivator)
    {
        if (TryFindConstructorInfo(implementationType, out var constructorInfo))
        {
            objectActivator = new MethodBaseActivator(constructorInfo!);
            return true;
        }

        objectActivator = null;
        return false;
    }

    private static bool TryFindConstructorInfo([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type implementationType, out ConstructorInfo? foundConstructorInfo)
    {
        var constructorInfos = implementationType.GetConstructors(ConstructorBindingFlags);
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