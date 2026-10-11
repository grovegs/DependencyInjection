using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using UnityEngine.SceneManagement;

namespace GroveGames.DependencyInjection.Unity
{
    public static class ContainerRegistry
    {
        private static readonly Dictionary<int, IContainer?> s_scenes = new();
        private static IContainer? s_root;

        public static IObjectResolver? Root => s_root;

        public static bool TryGet(Scene scene, [NotNullWhen(true)] out IObjectResolver? resolver)
        {
            if (!scene.isLoaded)
            {
                resolver = null;
                return false;
            }

            resolver = s_scenes.TryGetValue(scene.handle, out var container) ? container : s_root;
            return resolver != null;
        }

        internal static void SetRoot(IContainer? root)
        {
            s_root = root;
        }

        internal static void AddScene(Scene scene)
        {
            s_scenes[scene.handle] = null;
        }

        internal static void SetSceneContainer(Scene scene, IContainer container)
        {
            s_scenes[scene.handle] = container;
        }

        internal static void RemoveScene(Scene scene)
        {
            s_scenes.Remove(scene.handle);
        }

        internal static void Clear()
        {
            s_scenes.Clear();
            s_root = null;
        }
    }
}
