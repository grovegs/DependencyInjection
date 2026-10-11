using Microsoft.CodeAnalysis;

namespace GroveGames.DependencyInjection.Generator;

internal static class DiagnosticDescriptors
{
    private const string Category = "Usage";
    private const string HelpLinkUri = "https://github.com/grovegs/DependencyInjection#analyzers";

    public static readonly DiagnosticDescriptor AbstractImplementation = new(
        "GGDI001",
        "Registered implementation must be a concrete class",
        "'{0}' is abstract or an interface and cannot be constructed",
        Category,
        DiagnosticSeverity.Error,
        true,
        helpLinkUri: HelpLinkUri);

    public static readonly DiagnosticDescriptor NoPublicConstructor = new(
        "GGDI002",
        "Registered implementation must have a public constructor",
        "'{0}' has no public constructor the container can call",
        Category,
        DiagnosticSeverity.Error,
        true,
        helpLinkUri: HelpLinkUri);

    public static readonly DiagnosticDescriptor IgnoredInject = new(
        "GGDI003",
        "[Inject] member is never called",
        "'{0}' is marked [Inject] but is never called because it is {1}",
        Category,
        DiagnosticSeverity.Warning,
        true,
        helpLinkUri: HelpLinkUri);

    public static readonly DiagnosticDescriptor RefParameter = new(
        "GGDI004",
        "Injected parameters cannot be ref, in or out",
        "'{0}' has a ref, in or out parameter that the container cannot resolve",
        Category,
        DiagnosticSeverity.Error,
        true,
        helpLinkUri: HelpLinkUri);

    public static readonly DiagnosticDescriptor MultipleInjectConstructors = new(
        "GGDI005",
        "Only one constructor should be marked [Inject]",
        "'{0}' has more than one [Inject] constructor and only the first one is used",
        Category,
        DiagnosticSeverity.Warning,
        true,
        helpLinkUri: HelpLinkUri);

    public static readonly DiagnosticDescriptor ReflectionFallback = new(
        "GGDI006",
        "Registered type is constructed through reflection",
        "'{0}' is constructed through reflection instead of generated code",
        Category,
        DiagnosticSeverity.Info,
        false,
        helpLinkUri: HelpLinkUri);
}
