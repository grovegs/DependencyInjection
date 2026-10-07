using System;
using System.Collections.Generic;

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

        public static T Instantiate<T>(this IObjectResolver resolver, T prefab)
            where T : Component
        {
            var instance = Object.Instantiate(prefab);
            resolver.InjectGameObject(instance.gameObject);
            return instance;
        }

        public static T Instantiate<T>(this IObjectResolver resolver, T prefab, Transform parent)
            where T : Component
        {
            var instance = Object.Instantiate(prefab, parent);
            resolver.InjectGameObject(instance.gameObject);
            return instance;
        }

        public static GameObject Instantiate(this IObjectResolver resolver, GameObject prefab, Transform? parent = null)
        {
            var instance = Object.Instantiate(prefab, parent);
            resolver.InjectGameObject(instance);
            return instance;
        }
    }
}
