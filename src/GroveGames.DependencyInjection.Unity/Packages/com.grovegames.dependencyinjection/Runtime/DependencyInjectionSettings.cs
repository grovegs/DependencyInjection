using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace GroveGames.DependencyInjection.Unity
{
    public sealed class DependencyInjectionSettings : ScriptableObject
    {
        private const string ConfigName = "com.grovegames.dependencyinjection.settings";

        private static DependencyInjectionSettings? s_loaded;

        [SerializeField] private RootInstaller[] _rootInstallers = System.Array.Empty<RootInstaller>();

        public RootInstaller[] RootInstallers => _rootInstallers;

        private void OnEnable()
        {
            s_loaded = this;
        }

        public static DependencyInjectionSettings GetOrCreate()
        {
#if UNITY_EDITOR
            if (EditorBuildSettings.TryGetConfigObject<DependencyInjectionSettings>(ConfigName, out var settings) && settings != null)
            {
                return settings;
            }
#else
            if (s_loaded != null)
            {
                return s_loaded;
            }
#endif
            var defaultSettings = CreateInstance<DependencyInjectionSettings>();
            defaultSettings.name = ConfigName;
            return defaultSettings;
        }

        public static string GetConfigName() => ConfigName;
    }
}
