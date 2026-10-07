using UnityEngine;

using Object = UnityEngine.Object;

namespace GroveGames.DependencyInjection.Unity
{
    public static class ComponentRegistrationExtensions
    {
        public static IContainerBuilder AddSingletonFromPrefab<T>(this IContainerBuilder builder, T prefab)
            where T : Component
        {
            builder.AddSingleton(typeof(ComponentOwner<T>), resolver =>
            {
                var owner = new ComponentOwner<T>(Object.Instantiate(prefab));
                resolver.InjectGameObject(owner.Component.gameObject);
                return owner;
            });

            return builder.AddSingleton(typeof(T), resolver => ((ComponentOwner<T>)resolver.Resolve(typeof(ComponentOwner<T>))).Component);
        }

        public static IContainerBuilder AddSingletonOnNewGameObject<T>(this IContainerBuilder builder, string name)
            where T : Component
        {
            builder.AddSingleton(typeof(ComponentOwner<T>), resolver =>
            {
                var owner = new ComponentOwner<T>(new GameObject(name).AddComponent<T>());
                resolver.InjectGameObject(owner.Component.gameObject);
                return owner;
            });

            return builder.AddSingleton(typeof(T), resolver => ((ComponentOwner<T>)resolver.Resolve(typeof(ComponentOwner<T>))).Component);
        }
    }
}
