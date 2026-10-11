using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace GroveGames.DependencyInjection.Generated;

internal static class GeneratedActivators
{
    private static readonly Dictionary<Assembly, GeneratedActivatorRegistry?> s_registries = [];
    private static readonly object s_lock = new();

    public static bool TryGetConstructor(Type type, [NotNullWhen(true)] out GeneratedConstructor? constructor)
    {
        var registry = GetRegistry(type.Assembly);

        if (registry is null)
        {
            constructor = null;
            return false;
        }

        return registry.TryGetConstructor(type, out constructor);
    }

    public static bool TryGetInjectMethods(Type type, [NotNullWhen(true)] out List<GeneratedMethod>? methods)
    {
        var registry = GetRegistry(type.Assembly);

        if (registry is null)
        {
            methods = null;
            return false;
        }

        return registry.TryGetInjectMethods(type, out methods);
    }

    private static GeneratedActivatorRegistry? GetRegistry(Assembly assembly)
    {
        lock (s_lock)
        {
            if (!s_registries.TryGetValue(assembly, out var registry))
            {
                registry = CreateRegistry(assembly);
                s_registries.Add(assembly, registry);
            }

            return registry;
        }
    }

    private static GeneratedActivatorRegistry? CreateRegistry(Assembly assembly)
    {
        var attribute = assembly.GetCustomAttribute<GeneratedActivatorsAttribute>();

        if (attribute is null)
        {
            return null;
        }

        var provider = (IGeneratedActivatorProvider)Activator.CreateInstance(attribute.ProviderType)!;
        var registry = new GeneratedActivatorRegistry();
        provider.Register(registry);
        return registry;
    }
}
