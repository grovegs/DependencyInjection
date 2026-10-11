using System.Collections.Immutable;

using Microsoft.CodeAnalysis;

namespace GroveGames.DependencyInjection.SourceGenerator;

internal static class TypeAnalyzer
{
    public const string InjectAttributeName = "GroveGames.DependencyInjection.InjectAttribute";

    public static TypeModel? Analyze(INamedTypeSymbol type, Compilation compilation, bool includeConstructor)
    {
        if (type.TypeKind != TypeKind.Class || type.IsAbstract || type.IsStatic)
        {
            return null;
        }

        if (!SymbolEqualityComparer.Default.Equals(type.ContainingAssembly, compilation.Assembly)
            || ContainsTypeParameter(type)
            || !compilation.IsSymbolAccessibleWithin(type, compilation.Assembly))
        {
            return null;
        }

        var constructor = includeConstructor ? AnalyzeConstructor(type, compilation) : null;
        var injectMethods = AnalyzeInjectMethods(type, compilation);

        if (constructor is null && injectMethods.Length == 0)
        {
            return null;
        }

        return new TypeModel(type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat), constructor, injectMethods);
    }

    public static bool HasInjectConstructor(INamedTypeSymbol type)
    {
        foreach (var constructor in type.InstanceConstructors)
        {
            if (HasInjectAttribute(constructor))
            {
                return true;
            }
        }

        return false;
    }

    private static EquatableArray<string>? AnalyzeConstructor(INamedTypeSymbol type, Compilation compilation)
    {
        IMethodSymbol? selected = null;

        foreach (var constructor in type.InstanceConstructors)
        {
            if (constructor.DeclaredAccessibility != Accessibility.Public)
            {
                continue;
            }

            if (HasInjectAttribute(constructor))
            {
                selected = constructor;
                break;
            }

            if (selected is null || constructor.Parameters.Length > selected.Parameters.Length)
            {
                selected = constructor;
            }
        }

        return selected is null ? null : GetParameterTypes(selected, compilation);
    }

    private static EquatableArray<MethodModel> AnalyzeInjectMethods(INamedTypeSymbol type, Compilation compilation)
    {
        var methods = ImmutableArray.CreateBuilder<MethodModel>();
        var overridden = new HashSet<IMethodSymbol>(SymbolEqualityComparer.Default);

        for (var current = type; current is not null; current = current.BaseType)
        {
            foreach (var member in current.GetMembers())
            {
                if (member is not IMethodSymbol method
                    || method.MethodKind != MethodKind.Ordinary
                    || method.IsStatic
                    || method.DeclaredAccessibility != Accessibility.Public
                    || overridden.Contains(method))
                {
                    continue;
                }

                for (var baseMethod = method.OverriddenMethod; baseMethod is not null; baseMethod = baseMethod.OverriddenMethod)
                {
                    overridden.Add(baseMethod);
                }

                if (!IsInjectMethod(method))
                {
                    continue;
                }

                var parameterTypes = method.IsGenericMethod || !compilation.IsSymbolAccessibleWithin(method, compilation.Assembly)
                    ? null
                    : GetParameterTypes(method, compilation);

                if (parameterTypes is null)
                {
                    return new EquatableArray<MethodModel>([]);
                }

                methods.Add(new MethodModel(method.Name, parameterTypes.Value));
            }
        }

        return new EquatableArray<MethodModel>(methods.ToArray());
    }

    private static bool IsInjectMethod(IMethodSymbol method)
    {
        for (var current = method; current is not null; current = current.OverriddenMethod)
        {
            if (HasInjectAttribute(current))
            {
                return true;
            }
        }

        return false;
    }

    private static bool HasInjectAttribute(ISymbol symbol)
    {
        foreach (var attribute in symbol.GetAttributes())
        {
            if (attribute.AttributeClass?.ToDisplayString() == InjectAttributeName)
            {
                return true;
            }
        }

        return false;
    }

    private static EquatableArray<string>? GetParameterTypes(IMethodSymbol method, Compilation compilation)
    {
        var parameterTypes = new string[method.Parameters.Length];

        for (var i = 0; i < method.Parameters.Length; i++)
        {
            var parameter = method.Parameters[i];
            var parameterType = parameter.Type;

            if (parameter.RefKind != RefKind.None
                || parameterType.TypeKind is TypeKind.Pointer or TypeKind.FunctionPointer or TypeKind.Dynamic or TypeKind.Error
                || parameterType.IsRefLikeType
                || ContainsTypeParameter(parameterType)
                || !compilation.IsSymbolAccessibleWithin(parameterType, compilation.Assembly))
            {
                return null;
            }

            parameterTypes[i] = parameterType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        }

        return new EquatableArray<string>(parameterTypes);
    }

    private static bool ContainsTypeParameter(ITypeSymbol type)
    {
        switch (type)
        {
            case ITypeParameterSymbol:
                return true;
            case IArrayTypeSymbol array:
                return ContainsTypeParameter(array.ElementType);
            case INamedTypeSymbol named:
                if (named.IsUnboundGenericType || (named.ContainingType is not null && ContainsTypeParameter(named.ContainingType)))
                {
                    return true;
                }

                foreach (var argument in named.TypeArguments)
                {
                    if (ContainsTypeParameter(argument))
                    {
                        return true;
                    }
                }

                return false;
            default:
                return false;
        }
    }
}
