using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace GroveGames.DependencyInjection.Unity.Editor
{
    internal static class DependencyInjectionSettingsAsset
    {
        public const string AssetPath = "Assets/Settings/GroveGames/DependencyInjectionSettings.asset";

        private const string LegacyAssetPath = "Assets/Settings/Resources/GroveGames/DependencyInjectionSettings.asset";

        [InitializeOnLoadMethod]
        private static void MigrateLegacyAsset()
        {
            if (File.Exists(LegacyAssetPath) && !EditorBuildSettings.TryGetConfigObject(DependencyInjectionSettings.ConfigName, out DependencyInjectionSettings _))
            {
                EditorApplication.delayCall += () => GetOrCreate();
            }
        }

        public static DependencyInjectionSettings GetOrCreate()
        {
            if (!EditorBuildSettings.TryGetConfigObject(DependencyInjectionSettings.ConfigName, out DependencyInjectionSettings settings) || settings == null)
            {
                settings = LoadOrCreate();
                EditorBuildSettings.AddConfigObject(DependencyInjectionSettings.ConfigName, settings, true);
            }

            Preload(settings);
            return settings;
        }

        private static DependencyInjectionSettings LoadOrCreate()
        {
            CreateFolder();

            if (AssetDatabase.LoadAssetAtPath<DependencyInjectionSettings>(AssetPath) == null && AssetDatabase.LoadAssetAtPath<DependencyInjectionSettings>(LegacyAssetPath) != null)
            {
                AssetDatabase.MoveAsset(LegacyAssetPath, AssetPath);
            }

            var settings = AssetDatabase.LoadAssetAtPath<DependencyInjectionSettings>(AssetPath);

            if (settings != null)
            {
                return settings;
            }

            AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<DependencyInjectionSettings>(), AssetPath);
            AssetDatabase.SaveAssets();
            return AssetDatabase.LoadAssetAtPath<DependencyInjectionSettings>(AssetPath);
        }

        private static void Preload(DependencyInjectionSettings settings)
        {
            var preloaded = PlayerSettings.GetPreloadedAssets();
            var assets = new List<Object>(preloaded.Length + 1);
            var isPreloaded = false;

            for (var i = 0; i < preloaded.Length; i++)
            {
                var asset = preloaded[i];

                if (asset is DependencyInjectionSettings && asset != settings)
                {
                    continue;
                }

                isPreloaded |= asset == settings;
                assets.Add(asset);
            }

            if (!isPreloaded)
            {
                assets.Add(settings);
            }

            if (isPreloaded && assets.Count == preloaded.Length)
            {
                return;
            }

            PlayerSettings.SetPreloadedAssets(assets.ToArray());
        }

        private static void CreateFolder()
        {
            var current = "Assets";

            foreach (var part in AssetPath.Substring("Assets/".Length).Split('/'))
            {
                if (part.EndsWith(".asset", System.StringComparison.Ordinal))
                {
                    return;
                }

                var next = current + "/" + part;

                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, part);
                }

                current = next;
            }
        }
    }
}
