using UnityEngine;

namespace GroveGames.DependencyInjection.Unity
{
    public sealed class DependencyInjectionSettings : ScriptableObject
    {
        public const string ResourcePath = "GroveGames/DependencyInjectionSettings";

        [SerializeField] private RootInstaller? _rootInstaller;

        public RootInstaller? RootInstaller => _rootInstaller;

        public static DependencyInjectionSettings GetOrCreate()
        {
            var settings = Resources.Load<DependencyInjectionSettings>(ResourcePath);
            return settings != null ? settings : CreateInstance<DependencyInjectionSettings>();
        }
    }
}
