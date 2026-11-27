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
        DiagnosticDescriptors.ReflectionFallback,
        DiagnosticDescriptors.UncalledInjectMethod,
        DiagnosticDescriptors.HiddenLifecycle,
        DiagnosticDescriptors.OpenGeneric);

    private static readonly string[] s_initializationInterfaces =
    [
        "GroveGames.DependencyInjection.IAsyncPreInitializable",
        "GroveGames.DependencyInjection.IAsyncInitializable",
        "GroveGames.DependencyInjection.IAsyncPostInitializable",
        "GroveGames.DependencyInjection.IPreInitializable",
        "GroveGames.DependencyInjection.IInitializable",
        "GroveGames.DependencyInjection.IPostInitializable"
    ];

    private static readonly string[] s_updateInterfaces =
    [
        "GroveGames.DependencyInjection.IPreFrameUpdatable",
        "GroveGames.DependencyInjection.IFrameUpdatable",
        "GroveGames.DependencyInjection.IPostFrameUpdatable",
        "GroveGames.DependencyInjection.IPhysicsUpdatable"
    ];

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
            AnalyzeFactory(context, invocation);
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

        if (type.IsUnboundGenericType)
        {
            context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.OpenGeneric, location, name));
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

        ReportUncalledInjectMethod(context, type, location);
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

    private static void ReportUncalledInjectMethod(OperationAnalysisContext context, INamedTypeSymbol type, Location location)
    {
        for (var current = type; current is not null; current = current.BaseType)
        {
            foreach (var member in current.GetMembers())
            {
                if (member is IMethodSymbol { MethodKind: MethodKind.Ordinary, IsStatic: false } method && TypeAnalyzer.HasInjectAttribute(method))
                {
                    context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.UncalledInjectMethod, location, type.ToDisplayString(), method.Name));
                    return;
                }
            }
        }
    }

    private static void AnalyzeFactory(OperationAnalysisContext context, IInvocationOperation invocation)
    {
        var method = invocation.TargetMethod;

        if (!RegistrationMethods.IsSingletonFactory(method) || method.TypeArguments[0] is not INamedTypeSymbol serviceType)
        {
            return;
        }

        foreach (var argument in invocation.Arguments)
        {
            if (argument.Value is not IDelegateCreationOperation { Target: IAnonymousFunctionOperation function } || GetCreatedType(function.Body) is not { } createdType)
            {
                continue;
            }

            if (SymbolEqualityComparer.Default.Equals(createdType, serviceType))
            {
                return;
            }

            var hidden = FindHiddenLifecycle(context.Compilation, serviceType, createdType);

            if (hidden is not null)
            {
                context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.HiddenLifecycle, invocation.Syntax.GetLocation(), serviceType.ToDisplayString(), createdType.ToDisplayString(), hidden.Name));
            }
        }
    }

    private static INamedTypeSymbol? FindHiddenLifecycle(Compilation compilation, ITypeSymbol serviceType, ITypeSymbol createdType)
    {
        var isEntryPoint = false;
        INamedTypeSymbol? hiddenInitialization = null;

        foreach (var name in s_initializationInterfaces)
        {
            if (compilation.GetTypeByMetadataName(name) is not { } lifecycle)
            {
                continue;
            }

            isEntryPoint |= Implements(serviceType, lifecycle);

            if (hiddenInitialization is null && Implements(createdType, lifecycle))
            {
                hiddenInitialization = lifecycle;
            }
        }

        if (!isEntryPoint && hiddenInitialization is not null)
        {
            return hiddenInitialization;
        }

        foreach (var name in s_updateInterfaces)
        {
            if (compilation.GetTypeByMetadataName(name) is { } lifecycle && Implements(createdType, lifecycle) && !Implements(serviceType, lifecycle))
            {
                return lifecycle;
            }
        }

        return null;
    }

    private static ITypeSymbol? GetCreatedType(IBlockOperation body)
    {
        if (body.Operations.Length != 1 || body.Operations[0] is not IReturnOperation { ReturnedValue: { } value })
        {
            return null;
        }

        while (value is IConversionOperation conversion)
        {
            value = conversion.Operand;
        }

        return value is IObjectCreationOperation creation ? creation.Type : null;
    }

    private static bool Implements(ITypeSymbol type, INamedTypeSymbol contract)
    {
        if (SymbolEqualityComparer.Default.Equals(type, contract))
        {
            return true;
        }

        foreach (var implemented in type.AllInterfaces)
        {
            if (SymbolEqualityComparer.Default.Equals(implemented, contract))
            {
                return true;
            }
        }

        return false;
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
