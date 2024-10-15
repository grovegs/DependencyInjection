namespace GroveGames.DependencyInjection.Godot.Tests;

public class SettingsTests
{
    private const string RootInstallerPath = "grove_games/dependency_injection/root_installer";
    private const string DefaultRootInstallerPath = "res://RootInstaller.tscn";

    [Fact]
    public void CreateRootInstallerSetting_ShouldSetDefault_WhenSettingDoesNotExist()
    {
        // Arrange
        var projectSettingsMock = new Mock<IProjectSettings>();
        projectSettingsMock.Setup(ps => ps.HasSetting(RootInstallerPath)).Returns(false);
        projectSettingsMock.Setup(ps => ps.SetSetting(RootInstallerPath, DefaultRootInstallerPath));
        var settings = new Settings(projectSettingsMock.Object);

        // Act
        settings.CreateRootInstallerSetting();

        // Assert
        projectSettingsMock.Verify(ps => ps.SetSetting(RootInstallerPath, DefaultRootInstallerPath), Times.Once);
    }

    [Fact]
    public void CreateRootInstallerSetting_ShouldNotSetDefault_WhenSettingExists()
    {
        // Arrange
        var projectSettingsMock = new Mock<IProjectSettings>();
        projectSettingsMock.Setup(ps => ps.HasSetting(RootInstallerPath)).Returns(true);
        var settings = new Settings(projectSettingsMock.Object);

        // Act
        settings.CreateRootInstallerSetting();

        // Assert
        projectSettingsMock.Verify(ps => ps.SetSetting(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void GetRootInstallerSetting_ShouldReturnCorrectPath()
    {
        // Arrange
        var expectedPath = DefaultRootInstallerPath;
        var projectSettingsMock = new Mock<IProjectSettings>();
        projectSettingsMock.Setup(ps => ps.GetSetting(RootInstallerPath)).Returns(expectedPath);
        var settings = new Settings(projectSettingsMock.Object);

        // Act
        var result = settings.GetRootInstallerSetting();

        // Assert
        Assert.Equal(expectedPath, result);
    }
}
