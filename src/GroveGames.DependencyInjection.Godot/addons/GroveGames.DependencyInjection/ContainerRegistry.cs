using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

using Godot;

namespace GroveGames.DependencyInjection.Godot;

public static class ContainerRegistry
{
    private static readonly Dictionary<Node, SceneEntry> s_scenes = new();
    private static IContainer s_root;

    public static IObjectResolver Root => s_root;

    public static bool TryGet(Node node, [NotNullWhen(true)] out IObjectResolver resolver)
    {
        resolver = null;

        if (node == null || !GodotObject.IsInstanceValid(node) || !node.IsInsideTree())
        {
            return false;
        }

        var entry = FindScene(node);

        if (entry != null)
        {
            if (!IsReady(entry))
            {
                return false;
            }

            resolver = entry.Container;
            return resolver != null;
        }

        resolver = s_root;
        return resolver != null;
    }

    public static Task<bool> WhenReadyAsync(Node node)
    {
        if (node == null || !GodotObject.IsInstanceValid(node))
        {
            return Task.FromResult(false);
        }

        var entry = FindScene(node);

        if (entry != null)
        {
            return entry.Ready.Task;
        }

        return Task.FromResult(node.IsInsideTree());
    }

    internal static void SetRoot(IContainer root)
    {
        s_root = root;
    }

    internal static void AddScene(Node scope)
    {
        s_scenes.Add(scope, new SceneEntry());
    }

    internal static void SetSceneContainer(Node scope, IContainer container)
    {
        if (s_scenes.TryGetValue(scope, out var entry))
        {
            entry.Container = container;
        }
    }

    internal static void CompleteScene(Node scope, bool isReady)
    {
        if (s_scenes.TryGetValue(scope, out var entry))
        {
            entry.Ready.TrySetResult(isReady);
        }
    }

    internal static bool RemoveScene(Node scope, out IContainer container)
    {
        container = null;

        if (!s_scenes.Remove(scope, out var entry))
        {
            return false;
        }

        entry.Ready.TrySetResult(false);
        container = entry.Container;
        return true;
    }

    internal static void Clear()
    {
        foreach (var entry in s_scenes.Values)
        {
            entry.Ready.TrySetResult(false);
        }

        s_scenes.Clear();
        s_root = null;
    }

    private static SceneEntry FindScene(Node node)
    {
        for (var current = node; current != null; current = current.GetParent())
        {
            if (s_scenes.TryGetValue(current, out var entry))
            {
                return entry;
            }
        }

        return null;
    }

    private static bool IsReady(SceneEntry entry)
    {
        var ready = entry.Ready.Task;
        return ready.IsCompleted && ready.Result;
    }

    private sealed class SceneEntry
    {
        public readonly TaskCompletionSource<bool> Ready;
        public IContainer Container;

        public SceneEntry()
        {
            Ready = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        }
    }
}
