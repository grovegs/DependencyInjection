using System.Collections.Generic;
using System.IO;

using UnityEditor;
using UnityEditor.UIElements;

using UnityEngine;
using UnityEngine.UIElements;

namespace GroveGames.DependencyInjection.Unity.Editor
{
    internal static class DependencyInjectionSettingsProvider
    {
        private const string AssetPath = "Assets/Settings/DependencyInjectionSettings.asset";

        [SettingsProvider]
        public static SettingsProvider CreateProvider()
        {
            return new SettingsProvider("Project/GroveGames/Dependency Injection", SettingsScope.Project)
            {
                label = "Dependency Injection",
                activateHandler = (searchContext, rootElement) =>
                {
                    var serializedObject = new SerializedObject(GetOrCreateSettings());

                    var container = new VisualElement
                    {
                        style =
                        {
                            paddingLeft = 10,
                            paddingRight = 10,
                            paddingTop = 10,
                            paddingBottom = 10
                        }
                    };

                    container.Add(new Label("Dependency Injection Settings")
                    {
                        style =
                        {
                            fontSize = 19,
                            unityFontStyleAndWeight = FontStyle.Bold,
                            marginBottom = 10
                        }
                    });

                    container.Add(new PropertyField(serializedObject.FindProperty("_rootInstallers"), "Root Installers"));

                    rootElement.Add(container);
                    rootElement.Bind(serializedObject);
                },
                keywords = new HashSet<string>(new[] { "Dependency", "Injection", "Container", "Installer", "Grove Games" })
            };
        }

        private static DependencyInjectionSettings GetOrCreateSettings()
        {
            if (!EditorBuildSettings.TryGetConfigObject<DependencyInjectionSettings>(DependencyInjectionSettings.GetConfigName(), out var settings) || settings == null)
            {
                settings = FindSettings() ?? CreateSettings();
                EditorBuildSettings.AddConfigObject(DependencyInjectionSettings.GetConfigName(), settings, true);
            }

            AddToPreloadedAssets(settings);
            return settings;
        }

        private static DependencyInjectionSettings? FindSettings()
        {
            var guids = AssetDatabase.FindAssets($"t:{nameof(DependencyInjectionSettings)}");

            for (var i = 0; i < guids.Length; i++)
            {
                var settings = AssetDatabase.LoadAssetAtPath<DependencyInjectionSettings>(AssetDatabase.GUIDToAssetPath(guids[i]));

                if (settings != null)
                {
                    return settings;
                }
            }

            return null;
        }

        private static DependencyInjectionSettings CreateSettings()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(AssetPath)!);
            AssetDatabase.Refresh();

            var settings = ScriptableObject.CreateInstance<DependencyInjectionSettings>();
            AssetDatabase.CreateAsset(settings, AssetDatabase.GenerateUniqueAssetPath(AssetPath));
            AssetDatabase.SaveAssets();
            return settings;
        }

        private static void AddToPreloadedAssets(DependencyInjectionSettings settings)
        {
            var preloadedAssets = new List<Object>(PlayerSettings.GetPreloadedAssets());

            if (preloadedAssets.Contains(settings))
            {
                return;
            }

            preloadedAssets.RemoveAll(asset => asset is DependencyInjectionSettings);
            preloadedAssets.Add(settings);
            PlayerSettings.SetPreloadedAssets(preloadedAssets.ToArray());
        }
    }
}
