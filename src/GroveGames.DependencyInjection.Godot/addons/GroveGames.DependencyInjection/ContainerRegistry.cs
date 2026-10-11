using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using Godot;

namespace GroveGames.DependencyInjection.Godot;

public static class ContainerRegistry
{
    private static readonly Dictionary<Node, IContainer> s_scenes = new();
    private static IContainer s_root;

    public static IObjectResolver Root => s_root;

    public static bool TryGet(Node node, [NotNullWhen(true)] out IObjectResolver resolver)
    {
        resolver = null;

        if (node == null || !GodotObject.IsInstanceValid(node) || !node.IsInsideTree())
        {
            return false;
        }

        for (var current = node; current != null; current = current.GetParent())
        {
            if (s_scenes.TryGetValue(current, out var container))
            {
                resolver = container;
                return resolver != null;
            }
        }

        resolver = s_root;
        return resolver != null;
    }

    internal static void SetRoot(IContainer root)
    {
        s_root = root;
    }

    internal static void AddScene(Node scope)
    {
        s_scenes[scope] = null;
    }

    internal static void SetSceneContainer(Node scope, IContainer container)
    {
        s_scenes[scope] = container;
    }

    internal static void RemoveScene(Node scope)
    {
        s_scenes.Remove(scope);
    }

    internal static void Clear()
    {
        s_scenes.Clear();
        s_root = null;
    }
}
