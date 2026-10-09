using UnityEditor;
using UnityEngine;

namespace GroveGames.DependencyInjection.Unity.Editor
{
    internal static class DependencyInjectionSettingsAsset
    {
        public const string AssetPath = "Assets/Settings/Resources/" + DependencyInjectionSettings.ResourcePath + ".asset";

        public static DependencyInjectionSettings GetOrCreate()
        {
            var settings = AssetDatabase.LoadAssetAtPath<DependencyInjectionSettings>(AssetPath);

            if (settings != null)
            {
                return settings;
            }

            CreateFolder();
            AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<DependencyInjectionSettings>(), AssetPath);
            AssetDatabase.SaveAssets();
            return AssetDatabase.LoadAssetAtPath<DependencyInjectionSettings>(AssetPath);
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
