using System;

using NUnit.Framework;

using UnityEditor;

namespace GroveGames.DependencyInjection.Unity.Editor.Tests
{
    public sealed class DependencyInjectionSettingsBuildPreprocessorTests
    {
        [Test]
        public void OnPreprocessBuild_ConfiguredSettings_AddsSettingsToPreloadedAssets()
        {
            if (!EditorBuildSettings.TryGetConfigObject<DependencyInjectionSettings>(DependencyInjectionSettings.GetConfigName(), out var settings) || settings == null)
            {
                Assert.Ignore("No configured settings asset.");
            }

            var preloadedAssets = PlayerSettings.GetPreloadedAssets();
            PlayerSettings.SetPreloadedAssets(Array.FindAll(preloadedAssets, asset => asset is not DependencyInjectionSettings));

            try
            {
                new DependencyInjectionSettingsBuildPreprocessor().OnPreprocessBuild(null);

                CollectionAssert.Contains(PlayerSettings.GetPreloadedAssets(), settings);
            }
            finally
            {
                PlayerSettings.SetPreloadedAssets(preloadedAssets);
            }
        }
    }
}
