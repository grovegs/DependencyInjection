using UnityEngine;
using UnityEngine.Pool;

using Object = UnityEngine.Object;

namespace GroveGames.DependencyInjection.Unity
{
    public static class GameObjectResolverExtensions
    {
        private static Transform? s_staging;

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
            gameObject.transform.SetParent(GetStaging(), false);
            var instance = gameObject.AddComponent<T>();
            Activate(resolver, gameObject, null);
            return instance;
        }

        public static T Instantiate<T>(this IObjectResolver resolver, T prefab)
            where T : Component
        {
            var instance = Object.Instantiate(prefab, GetStaging(), false);
            Activate(resolver, instance.gameObject, null);
            return instance;
        }

        public static T Instantiate<T>(this IObjectResolver resolver, T prefab, Transform parent)
            where T : Component
        {
            var instance = Object.Instantiate(prefab, GetStaging(), false);
            Activate(resolver, instance.gameObject, parent);
            return instance;
        }

        public static GameObject Instantiate(this IObjectResolver resolver, GameObject prefab, Transform? parent = null)
        {
            var instance = Object.Instantiate(prefab, GetStaging(), false);
            Activate(resolver, instance, parent);
            return instance;
        }

        private static void Activate(IObjectResolver resolver, GameObject gameObject, Transform? parent)
        {
            resolver.AddDisposable(new GameObjectLifetime(gameObject));
            resolver.InjectGameObject(gameObject);
            gameObject.transform.SetParent(parent, false);

            if (parent == null && Application.isPlaying)
            {
                Object.DontDestroyOnLoad(gameObject);
            }
        }

        private static Transform GetStaging()
        {
            if (s_staging != null)
            {
                return s_staging;
            }

            var staging = new GameObject("DependencyInjectionStaging")
            {
                hideFlags = HideFlags.HideInHierarchy
            };

            staging.SetActive(false);

            if (Application.isPlaying)
            {
                Object.DontDestroyOnLoad(staging);
            }

            s_staging = staging.transform;
            return s_staging;
        }
    }
}
