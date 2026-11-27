using UnityEngine;

namespace GroveGames.DependencyInjection.Unity
{
    public sealed class UnityDependencyInjectionSettings : ScriptableObject, IDependencyInjectionSettings
    {
        public const string ConfigName = "com.grovegames.dependencyinjection.settings";

        private static UnityDependencyInjectionSettings? s_instance;

        [SerializeField] private RootInstaller? _rootInstaller;

        public RootInstaller? RootInstaller => _rootInstaller;
        IInstaller? IDependencyInjectionSettings.RootInstaller => _rootInstaller;

        public static UnityDependencyInjectionSettings GetOrCreate()
        {
#if UNITY_EDITOR
            if (UnityEditor.EditorBuildSettings.TryGetConfigObject(ConfigName, out UnityDependencyInjectionSettings settings) && settings != null)
            {
                return settings;
            }
#else
            if (s_instance == null)
            {
                var preloaded = Resources.FindObjectsOfTypeAll<UnityDependencyInjectionSettings>();
                s_instance = preloaded.Length > 0 ? preloaded[0] : null;
            }
#endif
            if (s_instance == null)
            {
                Debug.LogWarning("The dependency injection settings asset was not found, so the root container has no installer. Open Project Settings > GroveGames > Dependency Injection to create it.");
                s_instance = CreateInstance<UnityDependencyInjectionSettings>();
                s_instance.hideFlags = HideFlags.DontSave;
            }

            return s_instance;
        }
    }
}
