using Microsoft.CodeAnalysis;

namespace GroveGames.DependencyInjection.Generator;

internal static class RegistrationMethods
{
    private const string BuilderExtensionsName = "GroveGames.DependencyInjection.ContainerBuilderExtensions";
    private const string BuilderInterfaceName = "GroveGames.DependencyInjection.IContainerBuilder";
    private const string BuilderName = "GroveGames.DependencyInjection.ContainerBuilder";

    public static RegistrationKind Classify(IMethodSymbol method)
    {
        if (method.Name is not ("AddSingleton" or "AddTransient"))
        {
            return RegistrationKind.None;
        }

        var definition = method.ReducedFrom ?? method;
        var containingType = definition.ContainingType?.ToDisplayString();

        if (containingType == BuilderExtensionsName)
        {
            return definition.Parameters.Length == 1 && method.TypeArguments.Length > 0
                ? RegistrationKind.Generic
                : RegistrationKind.None;
        }

        if (containingType is BuilderInterfaceName or BuilderName)
        {
            return method.Parameters.Length == 2 && method.Parameters[1].Type.ToDisplayString() == "System.Type"
                ? RegistrationKind.TypeOf
                : RegistrationKind.None;
        }

        return RegistrationKind.None;
    }
}
