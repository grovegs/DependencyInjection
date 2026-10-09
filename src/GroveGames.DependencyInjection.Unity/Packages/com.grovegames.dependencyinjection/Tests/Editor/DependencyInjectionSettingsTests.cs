using NUnit.Framework;

namespace GroveGames.DependencyInjection.Unity.Editor.Tests
{
    public sealed class DependencyInjectionSettingsTests
    {
        [Test]
        public void GetOrCreate_ReturnsSettings()
        {
            var settings = DependencyInjectionSettings.GetOrCreate();

            Assert.IsNotNull(settings);
        }

        [Test]
        public void GetOrCreate_SettingsAsset_IsLoadedFromResources()
        {
            var asset = DependencyInjectionSettingsAsset.GetOrCreate();

            Assert.AreSame(asset, DependencyInjectionSettings.GetOrCreate());
        }
    }
}
