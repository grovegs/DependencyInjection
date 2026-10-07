using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using UnityEngine;
using UnityEngine.SceneManagement;

namespace GroveGames.DependencyInjection.Unity
{
    public static class ContainerBootstrapper
    {
        private static readonly Dictionary<int, IContainer> s_sceneContainers = new();
        private static readonly HashSet<int> s_pendingScenes = new();
        private static IContainer? s_root;
        private static Task? s_rootInitialization;

        public static IContainer? Root => s_root;

        public static bool TryGetSceneContainer(Scene scene, out IContainer? container)
        {
            return s_sceneContainers.TryGetValue(scene.handle, out container);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap()
        {
            Shutdown();

            var builder = new ContainerBuilder();
            var installers = DependencyInjectionSettings.GetOrCreate().RootInstallers;

            try
            {
                for (var i = 0; i < installers.Length; i++)
                {
                    var installer = installers[i];

                    if (installer == null)
                    {
                        Debug.LogError($"Root installer at index {i} is missing.");
                        continue;
                    }

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
            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.sceneUnloaded += OnSceneUnloaded;
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

        internal static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (s_sceneContainers.ContainsKey(scene.handle) || s_pendingScenes.Contains(scene.handle))
            {
                return;
            }

            var installers = FindSceneInstallers(scene);

            if (installers.Count == 0)
            {
                return;
            }

            s_pendingScenes.Add(scene.handle);
            _ = InitializeSceneAsync(scene, installers);
        }

        private static async Task InitializeSceneAsync(Scene scene, List<SceneInstaller> installers)
        {
            try
            {
                var rootInitialization = s_rootInitialization;

                if (rootInitialization == null)
                {
                    s_pendingScenes.Remove(scene.handle);
                    return;
                }

                await rootInitialization;
            }
            catch
            {
                s_pendingScenes.Remove(scene.handle);
                return;
            }

            var root = s_root;

            if (!s_pendingScenes.Remove(scene.handle) || root == null || root.IsDisposed || !scene.isLoaded)
            {
                return;
            }

            try
            {
                var container = root.CreateChild(builder =>
                {
                    for (var i = 0; i < installers.Count; i++)
                    {
                        installers[i].Install(builder);
                    }
                });

                s_sceneContainers[scene.handle] = container;
                await container.InitializeAsync();
                ContainerPlayerLoop.Add(container);
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }

        private static void OnSceneUnloaded(Scene scene)
        {
            DisposeSceneContainer(scene);
        }

        internal static void DisposeSceneContainer(Scene scene)
        {
            s_pendingScenes.Remove(scene.handle);

            if (!s_sceneContainers.Remove(scene.handle, out var container))
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
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
            Application.quitting -= Shutdown;
            ContainerPlayerLoop.Uninstall();
            s_sceneContainers.Clear();
            s_pendingScenes.Clear();
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
