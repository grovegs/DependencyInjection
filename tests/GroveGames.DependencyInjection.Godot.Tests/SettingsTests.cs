namespace GroveGames.DependencyInjection.Godot.Tests;

public class SettingsTests
{
    private const string RootInstallerPathKey = "grove_games/dependency_injection/root_installer";
    private const string DefaultRootInstallerPath = "res://RootInstaller.tres";

    [Fact]
    public void CreateRootInstallerSetting_ShouldSetSetting_WhenSettingDoesNotExist()
    {
        // Arrange
        var mockProjectSettings = new Mock<IProjectSettings>();
        mockProjectSettings
            .Setup(ps => ps.HasSetting(RootInstallerPathKey))
            .Returns(false);
        var settings = new Settings(mockProjectSettings.Object);

        // Act
        settings.CreateRootInstallerSetting();

        // Assert
        mockProjectSettings.Verify(ps => ps.SetSetting(RootInstallerPathKey, DefaultRootInstallerPath), Times.Once);
    }

    [Fact]
    public void CreateRootInstallerSetting_ShouldNotSetSetting_WhenSettingAlreadyExists()
    {
        // Arrange
        var mockProjectSettings = new Mock<IProjectSettings>();
        mockProjectSettings
            .Setup(ps => ps.HasSetting(RootInstallerPathKey))
            .Returns(true);
        var settings = new Settings(mockProjectSettings.Object);

        // Act
        settings.CreateRootInstallerSetting();

        // Assert
        mockProjectSettings.Verify(ps => ps.SetSetting(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void GetRootInstallerSetting_ShouldReturnSettingValue()
    {
        // Arrange
        var mockProjectSettings = new Mock<IProjectSettings>();
        mockProjectSettings
            .Setup(ps => ps.GetSetting<string>(RootInstallerPathKey))
            .Returns(DefaultRootInstallerPath);
        var settings = new Settings(mockProjectSettings.Object);

        // Act
        var result = settings.GetRootInstallerSetting();

        // Assert
        Assert.Equal(DefaultRootInstallerPath, result);
    }
}