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
            var gameObject = new GameObject(typeof(T).Name);
            gameObject.transform.SetParent(GameObjectStaging.Transform, false);
            var instance = gameObject.AddComponent<T>();
            Activate(resolver, gameObject, null);
            return instance;
        }

        public static T Instantiate<T>(this IObjectResolver resolver, T prefab)
            where T : Component
        {
            var instance = Object.Instantiate(prefab, GameObjectStaging.Transform, false);
            Activate(resolver, instance.gameObject, null);
            return instance;
        }

        public static T Instantiate<T>(this IObjectResolver resolver, T prefab, Transform parent)
            where T : Component
        {
            var instance = Object.Instantiate(prefab, GameObjectStaging.Transform, false);
            Activate(resolver, instance.gameObject, parent);
            return instance;
        }

        public static GameObject Instantiate(this IObjectResolver resolver, GameObject prefab, Transform? parent = null)
        {
            var instance = Object.Instantiate(prefab, GameObjectStaging.Transform, false);
            Activate(resolver, instance, parent);
            return instance;
        }

        private static void Activate(IObjectResolver resolver, GameObject gameObject, Transform? parent)
        {
            resolver.AddDisposable(new GameObjectLifetime(gameObject));
            resolver.InjectGameObject(gameObject);

            if (parent != null || !Application.isPlaying)
            {
                gameObject.transform.SetParent(parent, false);
                return;
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
        }
    }
}
