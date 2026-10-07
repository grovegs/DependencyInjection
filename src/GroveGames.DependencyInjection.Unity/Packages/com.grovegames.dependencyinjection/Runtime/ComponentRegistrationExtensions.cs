using UnityEngine;

using Object = UnityEngine.Object;

namespace GroveGames.DependencyInjection.Unity
{
    public static class ComponentRegistrationExtensions
    {
        public static IContainerBuilder AddSingletonFromPrefab<T>(this IContainerBuilder builder, T prefab, bool dontDestroyOnLoad = false)
            where T : Component
        {
            return builder.AddSingleton(typeof(T), resolver =>
            {
                var instance = Object.Instantiate(prefab);

                if (dontDestroyOnLoad)
                {
                    Object.DontDestroyOnLoad(instance.gameObject);
                }

                resolver.InjectGameObject(instance.gameObject);
                return instance;
            });
        }

        public static IContainerBuilder AddSingletonOnNewGameObject<T>(this IContainerBuilder builder, string name, bool dontDestroyOnLoad = false)
            where T : Component
        {
            return builder.AddSingleton(typeof(T), resolver =>
            {
                var gameObject = new GameObject(name);

                if (dontDestroyOnLoad)
                {
                    Object.DontDestroyOnLoad(gameObject);
                }

                var instance = gameObject.AddComponent<T>();
                resolver.InjectGameObject(gameObject);
                return instance;
            });
        }
    }
}
