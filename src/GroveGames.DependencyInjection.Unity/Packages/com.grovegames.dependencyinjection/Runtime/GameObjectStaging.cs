using UnityEngine;

using Object = UnityEngine.Object;

namespace GroveGames.DependencyInjection.Unity
{
    internal static class GameObjectStaging
    {
        private static Transform? s_transform;

        public static Transform Transform => s_transform != null ? s_transform : Create();

        private static Transform Create()
        {
            var gameObject = new GameObject(nameof(GameObjectStaging))
            {
                hideFlags = HideFlags.HideAndDontSave
            };

            gameObject.SetActive(false);

            if (Application.isPlaying)
            {
                Object.DontDestroyOnLoad(gameObject);
            }

            s_transform = gameObject.transform;
            return s_transform;
        }
    }
}
