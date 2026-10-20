using UnityEngine;

namespace GroveGames.DependencyInjection.Unity
{
    public sealed class DependencyInjectionSettings : ScriptableObject
    {
        public const string ConfigName = "com.grovegames.dependencyinjection.settings";

        private static DependencyInjectionSettings? s_instance;

        [SerializeField] private RootInstaller? _rootInstaller;

        public RootInstaller? RootInstaller => _rootInstaller;

        public static DependencyInjectionSettings GetOrCreate()
        {
#if UNITY_EDITOR
            if (UnityEditor.EditorBuildSettings.TryGetConfigObject(ConfigName, out DependencyInjectionSettings settings) && settings != null)
            {
                return settings;
            }
#else
            if (s_instance == null)
            {
                var preloaded = Resources.FindObjectsOfTypeAll<DependencyInjectionSettings>();
                s_instance = preloaded.Length > 0 ? preloaded[0] : null;
            }
#endif
            if (s_instance == null)
            {
                s_instance = CreateInstance<DependencyInjectionSettings>();
                s_instance.hideFlags = HideFlags.DontSave;
            }

            return s_instance;
        }
    }
}
