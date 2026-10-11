using System.Collections.Immutable;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace GroveGames.DependencyInjection.Generator;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class InjectionAnalyzer : DiagnosticAnalyzer
{
    private static readonly ImmutableArray<DiagnosticDescriptor> s_supportedDiagnostics = ImmutableArray.Create(
        DiagnosticDescriptors.AbstractImplementation,
        DiagnosticDescriptors.NoPublicConstructor,
        DiagnosticDescriptors.IgnoredInject,
        DiagnosticDescriptors.RefParameter,
        DiagnosticDescriptors.MultipleInjectConstructors,
        DiagnosticDescriptors.ReflectionFallback);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => s_supportedDiagnostics;

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(static context =>
        {
            if (context.Compilation.GetTypeByMetadataName(TypeAnalyzer.InjectAttributeName) is null)
            {
                return;
            }

            context.RegisterOperationAction(AnalyzeInvocation, OperationKind.Invocation);
            context.RegisterSymbolAction(AnalyzeMethod, SymbolKind.Method);
            context.RegisterSymbolAction(AnalyzeType, SymbolKind.NamedType);
        });
    }

    private static void AnalyzeInvocation(OperationAnalysisContext context)
    {
        var invocation = (IInvocationOperation)context.Operation;
        var method = invocation.TargetMethod;
        var implementationType = RegistrationMethods.Classify(method) switch
        {
            RegistrationKind.Generic => method.TypeArguments[method.TypeArguments.Length - 1],
            RegistrationKind.TypeOf => GetTypeOfArgument(invocation),
            _ => null
        };

        if (implementationType is null)
        {
            return;
        }

        var location = invocation.Syntax.GetLocation();
        var name = implementationType.ToDisplayString();

        if (implementationType is ITypeParameterSymbol)
        {
            context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.ReflectionFallback, location, name));
            return;
        }

        if (implementationType is not INamedTypeSymbol type)
        {
            return;
        }

        if (type.TypeKind == TypeKind.Interface || (type.TypeKind == TypeKind.Class && type.IsAbstract))
        {
            context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.AbstractImplementation, location, name));
            return;
        }

        if (type.TypeKind != TypeKind.Class)
        {
            return;
        }

        var constructor = TypeAnalyzer.SelectConstructor(type);

        if (constructor is null)
        {
            context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.NoPublicConstructor, location, name));
            return;
        }

        if (TypeAnalyzer.HasRefParameter(constructor))
        {
            if (!TypeAnalyzer.HasInjectAttribute(constructor))
            {
                context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.RefParameter, location, constructor.ToDisplayString()));
            }

            return;
        }

        if (TypeAnalyzer.Analyze(type, context.Compilation, true)?.ConstructorParameterTypes is null)
        {
            context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.ReflectionFallback, location, name));
        }
    }

    private static void AnalyzeMethod(SymbolAnalysisContext context)
    {
        var method = (IMethodSymbol)context.Symbol;

        if (!TypeAnalyzer.HasInjectAttribute(method))
        {
            return;
        }

        var reason = method.MethodKind switch
        {
            MethodKind.StaticConstructor => "static",
            MethodKind.Constructor or MethodKind.Ordinary when method.DeclaredAccessibility != Accessibility.Public => "not public",
            MethodKind.Ordinary when method.IsStatic => "static",
            MethodKind.Ordinary when method.IsGenericMethod => "generic",
            _ => null
        };

        if (reason is not null)
        {
            Report(context, DiagnosticDescriptors.IgnoredInject, method, method.ToDisplayString(), reason);
            return;
        }

        if (method.MethodKind is MethodKind.Constructor or MethodKind.Ordinary && TypeAnalyzer.HasRefParameter(method))
        {
            Report(context, DiagnosticDescriptors.RefParameter, method, method.ToDisplayString());
        }
    }

    private static void AnalyzeType(SymbolAnalysisContext context)
    {
        var type = (INamedTypeSymbol)context.Symbol;
        var injectConstructors = 0;

        foreach (var constructor in type.InstanceConstructors)
        {
            if (constructor.DeclaredAccessibility == Accessibility.Public && TypeAnalyzer.HasInjectAttribute(constructor))
            {
                injectConstructors++;
            }
        }

        if (injectConstructors > 1)
        {
            Report(context, DiagnosticDescriptors.MultipleInjectConstructors, type, type.ToDisplayString());
        }
    }

    private static ITypeSymbol? GetTypeOfArgument(IInvocationOperation invocation)
    {
        foreach (var argument in invocation.Arguments)
        {
            if (argument.Parameter?.Ordinal == 1 && argument.Value is ITypeOfOperation typeOf)
            {
                return typeOf.TypeOperand;
            }
        }

        return null;
    }

    private static void Report(SymbolAnalysisContext context, DiagnosticDescriptor descriptor, ISymbol symbol, params object[] arguments)
    {
        var location = symbol.Locations.Length > 0 ? symbol.Locations[0] : Location.None;
        context.ReportDiagnostic(Diagnostic.Create(descriptor, location, arguments));
    }
}
