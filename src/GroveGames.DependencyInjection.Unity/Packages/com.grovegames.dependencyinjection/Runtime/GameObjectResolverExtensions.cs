using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.SceneManagement;

using Object = UnityEngine.Object;

namespace GroveGames.DependencyInjection.Unity
{
    public static class GameObjectResolverExtensions
    {
        public static void InjectGameObject(this IObjectResolver resolver, GameObject gameObject)
        {
            if (gameObject == null)
            {
                Debug.LogError("GameObject cannot be null.");
                return;
            }

            var behaviours = ListPool<MonoBehaviour>.Get();

            try
            {
                gameObject.GetComponentsInChildren(true, behaviours);

                for (var i = 0; i < behaviours.Count; i++)
                {
                    var behaviour = behaviours[i];

                    if (behaviour != null)
                    {
                        resolver.Inject(behaviour);
                    }
                }
            }
            finally
            {
                ListPool<MonoBehaviour>.Release(behaviours);
            }
        }

        public static T Instantiate<T>(this IObjectResolver resolver)
            where T : Component
        {
            if (resolver == null)
            {
                Debug.LogError("The resolver cannot be null.");
                return null!;
            }

            var gameObject = new GameObject(typeof(T).Name);
            gameObject.transform.SetParent(GameObjectStaging.Transform, false);
            var instance = gameObject.AddComponent<T>();
            return Activate(resolver, gameObject, null) ? instance : null!;
        }

        public static T Instantiate<T>(this IObjectResolver resolver, T prefab)
            where T : Component
        {
            return Instantiate(resolver, prefab, null!);
        }

        public static T Instantiate<T>(this IObjectResolver resolver, T prefab, Transform parent)
            where T : Component
        {
            if (resolver == null || prefab == null)
            {
                Debug.LogError(resolver == null ? "The resolver cannot be null." : "The prefab cannot be null.");
                return null!;
            }

            var instance = Object.Instantiate(prefab, GameObjectStaging.Transform, false);
            return Activate(resolver, instance.gameObject, parent) ? instance : null!;
        }

        public static GameObject Instantiate(this IObjectResolver resolver, GameObject prefab, Transform? parent = null)
        {
            if (resolver == null || prefab == null)
            {
                Debug.LogError(resolver == null ? "The resolver cannot be null." : "The prefab cannot be null.");
                return null!;
            }

            var instance = Object.Instantiate(prefab, GameObjectStaging.Transform, false);
            return Activate(resolver, instance, parent) ? instance : null!;
        }

        // The clone waits under the hidden, inactive staging object, so one that cannot be tracked or injected is destroyed
        // instead of staying there until its container is disposed or for good.
        private static bool Activate(IObjectResolver resolver, GameObject gameObject, Transform? parent)
        {
            try
            {
                GameObjectLifetime.Track(resolver, gameObject);
                resolver.InjectGameObject(gameObject);
            }
            catch (System.Exception exception)
            {
                Debug.LogException(exception);

                if (Application.isPlaying)
                {
                    Object.Destroy(gameObject);
                }
                else
                {
                    Object.DestroyImmediate(gameObject);
                }

                return false;
            }

            if (parent != null || !Application.isPlaying)
            {
                gameObject.transform.SetParent(parent, false);
                return true;
            }

            var isActive = gameObject.activeSelf;
            gameObject.SetActive(false);
            gameObject.transform.SetParent(null, false);

            if (ContainerBootstrapper.TryGetScene(resolver, out var scene) && scene.isLoaded)
            {
                SceneManager.MoveGameObjectToScene(gameObject, scene);
            }
            else
            {
                Object.DontDestroyOnLoad(gameObject);
            }

            gameObject.SetActive(isActive);
            return true;
        }
    }
}
