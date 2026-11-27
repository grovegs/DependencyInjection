using Microsoft.CodeAnalysis;

namespace GroveGames.DependencyInjection.Generator;

internal static class RegistrationMethods
{
    private const string BuilderExtensionsName = "GroveGames.DependencyInjection.ContainerBuilderExtensions";
    private const string BuilderInterfaceName = "GroveGames.DependencyInjection.IContainerBuilder";
    private const string BuilderName = "GroveGames.DependencyInjection.ContainerBuilder";

    public static bool IsSingletonFactory(IMethodSymbol method)
    {
        var definition = (method.ReducedFrom ?? method).OriginalDefinition;

        return method.Name == "AddSingleton"
            && method.TypeArguments.Length == 1
            && definition.ContainingType?.ToDisplayString() == BuilderExtensionsName
            && definition.Parameters.Length == 2
            && definition.Parameters[1].Type is INamedTypeSymbol { Name: "Func", TypeArguments.Length: 2 } factory
            && factory.TypeArguments[0].ToDisplayString() == "GroveGames.DependencyInjection.IObjectResolver";
    }

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
