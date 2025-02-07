using Godot;

namespace GroveGames.DependencyInjection;

public static class GodotRootContainerFactory
{
    public static GodotRootContainer CreateGodotRootContainer()
    {
        var rootInstallerPath = GodotSettings.RootInstaller.Value;
        var rootInstallerResource = ResourceLoader.Load<Resource>(rootInstallerPath);

        if (rootInstallerResource is not IRootInstaller rootInstaller)
        {
            throw new RootInstallerNotFoundException(rootInstallerPath);
        }

        var container = RootContainerFactory.CreateRootContainer(rootInstaller.Install);
        var godotContainer = new GodotRootContainer(container);

        if (Engine.GetMainLoop() is not SceneTree sceneTree)
        {
            throw new SceneTreeNotFoundException();
        }

        var root = sceneTree.Root;
        root.CallDeferred(Node.MethodName.AddChild, godotContainer);
        root.CloseRequested += godotContainer.Dispose;
        return godotContainer;
    }
}
