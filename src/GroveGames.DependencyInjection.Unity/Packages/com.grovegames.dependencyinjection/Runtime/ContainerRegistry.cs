using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

using UnityEngine.SceneManagement;

namespace GroveGames.DependencyInjection.Unity
{
    public static class ContainerRegistry
    {
        private sealed class SceneEntry
        {
            public readonly TaskCompletionSource<bool> Ready;
            public IContainer? Container;

            public SceneEntry()
            {
                Ready = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            }
        }

        private static readonly Dictionary<int, SceneEntry> s_scenes = new();
        private static readonly Dictionary<IContainer, Scene> s_containerScenes = new();
        private static IContainer? s_root;

        public static IObjectResolver? Root => s_root;

        public static bool TryGet(Scene scene, [NotNullWhen(true)] out IObjectResolver? resolver)
        {
            resolver = null;

            if (!scene.isLoaded)
            {
                return false;
            }

            if (s_scenes.TryGetValue(scene.handle, out var entry))
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

        public static Task<bool> WhenReadyAsync(Scene scene)
        {
            if (s_scenes.TryGetValue(scene.handle, out var entry))
            {
                return entry.Ready.Task;
            }

            return Task.FromResult(scene.isLoaded);
        }

        internal static void SetRoot(IContainer? root)
        {
            s_root = root;
        }

        internal static void AddScene(Scene scene)
        {
            s_scenes.Add(scene.handle, new SceneEntry());
        }

        internal static void SetSceneContainer(Scene scene, IContainer container)
        {
            if (s_scenes.TryGetValue(scene.handle, out var entry))
            {
                entry.Container = container;
                s_containerScenes[container] = scene;
            }
        }

        internal static void CompleteScene(Scene scene, bool isReady)
        {
            if (s_scenes.TryGetValue(scene.handle, out var entry))
            {
                entry.Ready.TrySetResult(isReady);
            }
        }

        internal static bool RemoveScene(Scene scene, out IContainer? container)
        {
            container = null;

            if (!s_scenes.Remove(scene.handle, out var entry))
            {
                return false;
            }

            entry.Ready.TrySetResult(false);
            container = entry.Container;

            if (container != null)
            {
                s_containerScenes.Remove(container);
            }

            return true;
        }

        internal static bool TryGetScene(IObjectResolver resolver, out Scene scene)
        {
            for (var container = resolver as IContainer; container != null; container = container.Parent)
            {
                if (s_containerScenes.TryGetValue(container, out scene))
                {
                    return true;
                }
            }

            scene = default;
            return false;
        }

        internal static void Clear()
        {
            foreach (var entry in s_scenes.Values)
            {
                entry.Ready.TrySetResult(false);
            }

            s_scenes.Clear();
            s_containerScenes.Clear();
            s_root = null;
        }

        private static bool IsReady(SceneEntry entry)
        {
            var ready = entry.Ready.Task;
            return ready.IsCompleted && ready.Result;
        }
    }
}
