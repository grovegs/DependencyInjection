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
        public void GetConfigName_ReturnsPackageScopedName()
        {
            var configName = DependencyInjectionSettings.GetConfigName();

            Assert.AreEqual("com.grovegames.dependencyinjection.settings", configName);
        }
    }
}
