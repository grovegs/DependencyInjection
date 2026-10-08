using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using UnityEngine;
using UnityEngine.SceneManagement;

namespace GroveGames.DependencyInjection.Unity
{
    public static class ContainerBootstrapper
    {
        private sealed class SceneState
        {
            public readonly Scene Scene;
            public readonly List<SceneInstaller> Installers;
            public readonly List<GameObject> PausedRoots;
            public readonly TaskCompletionSource<bool> Ready;
            public IContainer? Container;
            public bool IsStarted;

            public SceneState(Scene scene, List<SceneInstaller> installers, List<GameObject> pausedRoots)
            {
                Scene = scene;
                Installers = installers;
                PausedRoots = pausedRoots;
                Ready = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            }
        }

        private static readonly Dictionary<int, SceneState> s_scenes = new();
        private static readonly List<SceneState> s_pendingScenes = new();
        private static IContainer? s_root;
        private static Task? s_rootInitialization;

        public static IContainer? Root => s_root;

        public static bool TryGetContainer(Scene scene, out IContainer? container)
        {
            container = null;

            if (!scene.isLoaded)
            {
                return false;
            }

            if (s_scenes.TryGetValue(scene.handle, out var state))
            {
                var ready = state.Ready.Task;

                if (!ready.IsCompleted || !ready.Result)
                {
                    return false;
                }

                container = state.Container;
                return container != null;
            }

            var root = s_root;

            if (root == null || !root.IsInitialized)
            {
                return false;
            }

            container = root;
            return true;
        }

        public static Task<bool> WhenSceneReadyAsync(Scene scene)
        {
            if (s_scenes.TryGetValue(scene.handle, out var state))
            {
                return state.Ready.Task;
            }

            return Task.FromResult(scene.isLoaded);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap()
        {
            Shutdown();

            var builder = new ContainerBuilder();
            var installer = DependencyInjectionSettings.GetOrCreate().RootInstaller;

            try
            {
                if (installer != null)
                {
                    installer.Install(builder);
                }

                s_root = builder.Build();
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                return;
            }

            ContainerPlayerLoop.Install();
            SceneManager.activeSceneChanged += OnActiveSceneChanged;
            Application.quitting += Shutdown;
            s_rootInitialization = InitializeRootAsync(s_root);
        }

        private static async Task InitializeRootAsync(IContainer root)
        {
            try
            {
                await root.InitializeAsync();
                ContainerPlayerLoop.Add(root);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                throw;
            }
        }

        internal static void InitializeScene(Scene scene)
        {
            if (!scene.IsValid() || s_scenes.ContainsKey(scene.handle))
            {
                return;
            }

            var installers = FindSceneInstallers(scene);

            if (installers.Count == 0)
            {
                return;
            }

            var state = new SceneState(scene, installers, PauseScene(scene));
            s_scenes.Add(scene.handle, state);

            if (scene == SceneManager.GetActiveScene())
            {
                Start(state);
                return;
            }

            s_pendingScenes.Add(state);
        }

        internal static void StartPendingScenes()
        {
            if (s_pendingScenes.Count == 0)
            {
                return;
            }

            var pendingScenes = s_pendingScenes.ToArray();
            s_pendingScenes.Clear();

            for (var i = 0; i < pendingScenes.Length; i++)
            {
                if (IsCurrent(pendingScenes[i]))
                {
                    Start(pendingScenes[i]);
                }
            }
        }

        private static void OnActiveSceneChanged(Scene previous, Scene next)
        {
            if (s_scenes.TryGetValue(next.handle, out var state))
            {
                Start(state);
            }
        }

        private static void Start(SceneState state)
        {
            if (state.IsStarted)
            {
                return;
            }

            state.IsStarted = true;
            s_pendingScenes.Remove(state);
            _ = InitializeSceneAsync(state);
        }

        private static async Task InitializeSceneAsync(SceneState state)
        {
            var rootInitialization = s_rootInitialization;

            if (rootInitialization == null)
            {
                Debug.LogError($"Scene '{state.Scene.name}' was started without its container because the root container is not available.");
                Complete(state, false);
                return;
            }

            try
            {
                await rootInitialization;
            }
            catch
            {
                if (IsCurrent(state))
                {
                    Debug.LogError($"Scene '{state.Scene.name}' was started without its container because the root container failed to initialize.");
                    Complete(state, false);
                }

                return;
            }

            if (!IsCurrent(state))
            {
                return;
            }

            var root = s_root;

            if (root == null || root.IsDisposed || !state.Scene.isLoaded)
            {
                Complete(state, false);
                return;
            }

            try
            {
                var installers = state.Installers;
                var container = root.CreateChild(builder =>
                {
                    for (var i = 0; i < installers.Count; i++)
                    {
                        installers[i].Install(builder);
                    }
                });

                installers.Clear();
                state.Container = container;
                InjectScene(container, state.Scene);
                await container.InitializeAsync();

                if (!IsCurrent(state))
                {
                    return;
                }

                ContainerPlayerLoop.Add(container);
                Complete(state, true);
            }
            catch (Exception exception)
            {
                if (!IsCurrent(state))
                {
                    return;
                }

                Debug.LogException(exception);
                Debug.LogError($"Scene '{state.Scene.name}' was started without a fully initialized container.");
                Complete(state, false);
            }
        }

        private static bool IsCurrent(SceneState state)
        {
            return s_scenes.TryGetValue(state.Scene.handle, out var current) && ReferenceEquals(current, state);
        }

        private static void Complete(SceneState state, bool isReady)
        {
            ResumeScene(state.PausedRoots);
            state.Ready.TrySetResult(isReady);
        }

        internal static bool TryGetScene(IObjectResolver resolver, out Scene scene)
        {
            for (var container = resolver as IContainer; container != null; container = container.Parent)
            {
                foreach (var state in s_scenes.Values)
                {
                    if (ReferenceEquals(state.Container, container))
                    {
                        scene = state.Scene;
                        return true;
                    }
                }
            }

            scene = default;
            return false;
        }

        internal static void DisposeSceneContainer(Scene scene)
        {
            if (!s_scenes.Remove(scene.handle, out var state))
            {
                return;
            }

            state.Ready.TrySetResult(false);
            var container = state.Container;

            if (container == null)
            {
                return;
            }

            ContainerPlayerLoop.Remove(container);

            try
            {
                container.Dispose();
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }

        private static List<GameObject> PauseScene(Scene scene)
        {
            var rootObjects = new List<GameObject>();
            var pausedRoots = new List<GameObject>();
            scene.GetRootGameObjects(rootObjects);

            for (var i = 0; i < rootObjects.Count; i++)
            {
                var rootObject = rootObjects[i];

                if (rootObject.activeSelf)
                {
                    pausedRoots.Add(rootObject);
                    rootObject.SetActive(false);
                }
            }

            return pausedRoots;
        }

        private static void ResumeScene(List<GameObject> pausedRoots)
        {
            for (var i = 0; i < pausedRoots.Count; i++)
            {
                var rootObject = pausedRoots[i];

                if (rootObject != null)
                {
                    rootObject.SetActive(true);
                }
            }

            pausedRoots.Clear();
        }

        private static void InjectScene(IContainer container, Scene scene)
        {
            var rootObjects = new List<GameObject>();
            scene.GetRootGameObjects(rootObjects);

            for (var i = 0; i < rootObjects.Count; i++)
            {
                container.InjectGameObject(rootObjects[i]);
            }
        }

        private static List<SceneInstaller> FindSceneInstallers(Scene scene)
        {
            var installers = new List<SceneInstaller>();
            var rootObjects = new List<GameObject>();
            var buffer = new List<SceneInstaller>();
            scene.GetRootGameObjects(rootObjects);

            for (var i = 0; i < rootObjects.Count; i++)
            {
                rootObjects[i].GetComponentsInChildren(true, buffer);
                installers.AddRange(buffer);
            }

            return installers;
        }

        private static void Shutdown()
        {
            SceneManager.activeSceneChanged -= OnActiveSceneChanged;
            Application.quitting -= Shutdown;
            s_pendingScenes.Clear();
            ContainerPlayerLoop.Uninstall();

            foreach (var state in s_scenes.Values)
            {
                state.Ready.TrySetResult(false);
            }

            s_scenes.Clear();
            s_rootInitialization = null;
            var root = s_root;
            s_root = null;

            if (root == null)
            {
                return;
            }

            try
            {
                root.Dispose();
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }
    }
}
