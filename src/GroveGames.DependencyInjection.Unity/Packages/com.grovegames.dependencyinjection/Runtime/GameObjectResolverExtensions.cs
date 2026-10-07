using UnityEngine;
using UnityEngine.Pool;

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
            var instance = gameObject.AddComponent<T>();
            Own(resolver, gameObject, true);
            resolver.InjectGameObject(gameObject);
            return instance;
        }

        public static T Instantiate<T>(this IObjectResolver resolver, T prefab)
            where T : Component
        {
            var instance = Object.Instantiate(prefab);
            Own(resolver, instance.gameObject, true);
            resolver.InjectGameObject(instance.gameObject);
            return instance;
        }

        public static T Instantiate<T>(this IObjectResolver resolver, T prefab, Transform parent)
            where T : Component
        {
            var instance = Object.Instantiate(prefab, parent);
            Own(resolver, instance.gameObject, false);
            resolver.InjectGameObject(instance.gameObject);
            return instance;
        }

        public static GameObject Instantiate(this IObjectResolver resolver, GameObject prefab, Transform? parent = null)
        {
            var instance = Object.Instantiate(prefab, parent);
            Own(resolver, instance, parent == null);
            resolver.InjectGameObject(instance);
            return instance;
        }

        private static void Own(IObjectResolver resolver, GameObject gameObject, bool dontDestroyOnLoad)
        {
            if (dontDestroyOnLoad)
            {
                Object.DontDestroyOnLoad(gameObject);
            }

            resolver.AddDisposable(new GameObjectLifetime(gameObject));
        }
    }
}
