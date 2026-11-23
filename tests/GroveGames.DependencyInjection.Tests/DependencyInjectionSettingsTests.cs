namespace GroveGames.DependencyInjection.Tests;

public sealed class DependencyInjectionSettingsTests
{
    private sealed class TestInstaller : IInstaller
    {
        public void Install(IContainerBuilder builder)
        {
        }
    }

    [Fact]
    public void Constructor_NullRootInstaller_ExposesNull()
    {
        var settings = new DependencyInjectionSettings(null);

        Assert.Null(settings.RootInstaller);
    }

    [Fact]
    public void Constructor_RootInstaller_ExposesIt()
    {
        var installer = new TestInstaller();

        var settings = new DependencyInjectionSettings(installer);

        Assert.Same(installer, settings.RootInstaller);
    }
}
