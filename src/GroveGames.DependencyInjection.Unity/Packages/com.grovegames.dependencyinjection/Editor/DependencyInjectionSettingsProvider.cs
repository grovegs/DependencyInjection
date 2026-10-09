using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace GroveGames.DependencyInjection.Unity.Editor
{
    internal static class DependencyInjectionSettingsProvider
    {
        [SettingsProvider]
        public static SettingsProvider CreateProvider()
        {
            return new SettingsProvider("Project/GroveGames/Dependency Injection", SettingsScope.Project)
            {
                label = "Dependency Injection",
                activateHandler = (searchContext, rootElement) =>
                {
                    var settings = DependencyInjectionSettingsAsset.GetOrCreate();
                    var serializedObject = new SerializedObject(settings);

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

                    var assetField = new ObjectField("Settings Asset")
                    {
                        objectType = typeof(DependencyInjectionSettings),
                        value = settings,
                        style = { marginBottom = 10 }
                    };
                    assetField.SetEnabled(false);
                    container.Add(assetField);

                    container.Add(new PropertyField(serializedObject.FindProperty("_rootInstaller"), "Root Installer"));

                    rootElement.Add(container);
                    rootElement.Bind(serializedObject);
                },
                keywords = new HashSet<string>(new[] { "Dependency", "Injection", "Container", "Installer", "Grove Games" })
            };
        }
    }
}
