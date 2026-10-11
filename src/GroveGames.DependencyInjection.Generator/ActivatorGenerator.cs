using System.Text;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace GroveGames.DependencyInjection.Generator;

[Generator(LanguageNames.CSharp)]
public sealed class ActivatorGenerator : IIncrementalGenerator
{
    private const string ProviderInterfaceName = "GroveGames.DependencyInjection.Generated.IGeneratedActivatorProvider";
    private const string BuilderExtensionsName = "GroveGames.DependencyInjection.ContainerBuilderExtensions";
    private const string BuilderInterfaceName = "GroveGames.DependencyInjection.IContainerBuilder";
    private const string BuilderName = "GroveGames.DependencyInjection.ContainerBuilder";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var registeredTypes = context.SyntaxProvider
            .CreateSyntaxProvider(static (node, _) => IsRegistration(node), static (context, cancellationToken) => AnalyzeRegistration(context, cancellationToken))
            .Where(static model => model is not null)
            .Select(static (model, _) => model!);

        var injectableTypes = context.SyntaxProvider
            .CreateSyntaxProvider(static (node, _) => IsInjectableCandidate(node), static (context, cancellationToken) => AnalyzeInjectable(context, cancellationToken))
            .Where(static model => model is not null)
            .Select(static (model, _) => model!);

        var providerName = context.CompilationProvider.Select(static (compilation, _) => GetProviderName(compilation));
        var source = registeredTypes.Collect().Combine(injectableTypes.Collect()).Combine(providerName);

        context.RegisterSourceOutput(source, static (context, source) =>
        {
            var providerName = source.Right;

            if (providerName is null)
            {
                return;
            }

            var code = ActivatorEmitter.Emit(providerName, source.Left.Left, source.Left.Right);

            if (code is not null)
            {
                context.AddSource("GroveGames.DependencyInjection.GeneratedActivators.g.cs", code);
            }
        });
    }

    private static bool IsRegistration(SyntaxNode node)
    {
        if (node is not InvocationExpressionSyntax invocation)
        {
            return false;
        }

        var name = invocation.Expression switch
        {
            MemberAccessExpressionSyntax memberAccess => memberAccess.Name.Identifier.ValueText,
            SimpleNameSyntax simpleName => simpleName.Identifier.ValueText,
            _ => null
        };

        return name is "AddSingleton" or "AddTransient";
    }

    private static TypeModel? AnalyzeRegistration(GeneratorSyntaxContext context, CancellationToken cancellationToken)
    {
        var invocation = (InvocationExpressionSyntax)context.Node;

        if (context.SemanticModel.GetSymbolInfo(invocation, cancellationToken).Symbol is not IMethodSymbol method)
        {
            return null;
        }

        var definition = method.ReducedFrom ?? method;
        var containingType = definition.ContainingType?.ToDisplayString();
        ITypeSymbol? implementationType = null;

        if (containingType == BuilderExtensionsName)
        {
            if (definition.Parameters.Length == 1 && method.TypeArguments.Length > 0)
            {
                implementationType = method.TypeArguments[method.TypeArguments.Length - 1];
            }
        }
        else if (containingType is BuilderInterfaceName or BuilderName)
        {
            var arguments = invocation.ArgumentList.Arguments;

            if (method.Parameters.Length == 2
                && method.Parameters[1].Type.ToDisplayString() == "System.Type"
                && arguments.Count == 2
                && arguments[1].NameColon is null
                && arguments[1].Expression is TypeOfExpressionSyntax typeOf)
            {
                implementationType = context.SemanticModel.GetTypeInfo(typeOf.Type, cancellationToken).Type;
            }
        }

        return implementationType is INamedTypeSymbol namedType
            ? TypeAnalyzer.Analyze(namedType, context.SemanticModel.Compilation, true)
            : null;
    }

    private static bool IsInjectableCandidate(SyntaxNode node)
    {
        if (node is not ClassDeclarationSyntax declaration)
        {
            return false;
        }

        if (declaration.BaseList is not null)
        {
            return true;
        }

        foreach (var member in declaration.Members)
        {
            if (member is BaseMethodDeclarationSyntax { AttributeLists.Count: > 0 })
            {
                return true;
            }
        }

        return false;
    }

    private static TypeModel? AnalyzeInjectable(GeneratorSyntaxContext context, CancellationToken cancellationToken)
    {
        if (context.SemanticModel.GetDeclaredSymbol(context.Node, cancellationToken) is not INamedTypeSymbol type)
        {
            return null;
        }

        return TypeAnalyzer.Analyze(type, context.SemanticModel.Compilation, TypeAnalyzer.HasInjectConstructor(type));
    }

    private static string? GetProviderName(Compilation compilation)
    {
        if (compilation.GetTypeByMetadataName(ProviderInterfaceName) is null)
        {
            return null;
        }

        var assemblyName = compilation.AssemblyName ?? string.Empty;
        var builder = new StringBuilder("GeneratedActivatorProvider_", 27 + assemblyName.Length);

        foreach (var character in assemblyName)
        {
            builder.Append(char.IsLetterOrDigit(character) ? character : '_');
        }

        return builder.ToString();
    }
}
