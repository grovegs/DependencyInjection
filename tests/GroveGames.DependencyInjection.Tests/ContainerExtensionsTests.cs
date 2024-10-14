using GroveGames.DependencyInjection.Caching;

namespace GroveGames.DependencyInjection.Tests;

public class ContainerExtensionsTests
{
    [Fact]
    public void AddChild_WithContainerCacheAndConfigure_InvokesCorrectConfigureAction()
    {
        // Arrange
        var containerMock = new Mock<IContainer>();
        var cacheMock = new Mock<IContainerCache>();
        var configurerMock = new Mock<IContainerConfigurer>();
        var configured = false;

        void Configure(IContainerConfigurer c)
        {
            configured = true;
        }

        // Act
        var result = containerMock.Object.AddChild("testName", cacheMock.Object, Configure);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("testName", result.Name);
        Assert.True(configured);
    }

    [Fact]
    public void AddChild_WithContainerCacheAndInstaller_CallsInstallerInstallMethod()
    {
        // Arrange
        var containerMock = new Mock<IContainer>();
        var cacheMock = new Mock<IContainerCache>();
        var installerMock = new Mock<IInstaller>();

        // Act
        var result = containerMock.Object.AddChild("testName", cacheMock.Object, installerMock.Object);

        // Assert
        installerMock.Verify(i => i.Install(It.IsAny<IContainerConfigurer>()), Times.Once);
        Assert.NotNull(result);
        Assert.Equal("testName", result.Name);
    }

    [Fact]
    public void AddChild_WithConfigure_UsesSharedCache()
    {
        // Arrange
        var containerMock = new Mock<IContainer>();
        var configured = false;

        void Configure(IContainerConfigurer c)
        {
            configured = true;
        }

        // Act
        var result = containerMock.Object.AddChild("testName", Configure);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("testName", result.Name);
        Assert.True(configured);
        ContainerCache.Shared.Clear();
    }

    [Fact]
    public void AddChild_WithInstaller_UsesSharedCacheAndCallsInstall()
    {
        // Arrange
        var containerMock = new Mock<IContainer>();
        var installerMock = new Mock<IInstaller>();

        // Act
        var result = containerMock.Object.AddChild("testName", installerMock.Object);

        // Assert
        installerMock.Verify(i => i.Install(It.IsAny<IContainerConfigurer>()), Times.Once);
        Assert.NotNull(result);
        Assert.Equal("testName", result.Name);
        ContainerCache.Shared.Clear();
    }
}
