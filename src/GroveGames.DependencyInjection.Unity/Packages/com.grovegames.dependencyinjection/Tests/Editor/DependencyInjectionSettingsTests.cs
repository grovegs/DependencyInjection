using NUnit.Framework;
using UnityEditor;

namespace GroveGames.DependencyInjection.Unity.Editor.Tests
{
    public sealed class DependencyInjectionSettingsTests
    {
        [Test]
        public void GetOrCreate_ReturnsSettings()
        {
            var settings = UnityDependencyInjectionSettings.GetOrCreate();

            Assert.IsNotNull(settings);
        }

        [Test]
        public void GetOrCreate_SettingsAsset_IsReturnedAtRuntime()
        {
            var asset = DependencyInjectionSettingsAsset.GetOrCreate();

            var settings = UnityDependencyInjectionSettings.GetOrCreate();

            Assert.AreSame(asset, settings);
        }

        [Test]
        public void GetOrCreate_SettingsAsset_IsPreloaded()
        {
            var asset = DependencyInjectionSettingsAsset.GetOrCreate();

            var preloaded = PlayerSettings.GetPreloadedAssets();

            Assert.Contains(asset, preloaded);
        }
    }
}
