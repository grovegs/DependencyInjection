using Godot;

namespace GroveGames.DependencyInjection;

public sealed partial class DependencyInjector : Node
{
    public override void _Ready()
    {
        var settings = new Settings(new GodotProjectSettings());
        var rootInstallerPath = settings.GetRootInstallerSetting();
        var rootInstallerResource = ResourceLoader.Load<Resource>(rootInstallerPath);

        if (rootInstallerResource is not IRootInstaller rootInstaller)
        {
            throw new RootInstallerNotFoundException(rootInstallerPath);
        }

        var rootContainer = RootContainer.Create(rootInstaller.Install);
        var root = GetTree().Root;
        root.TreeExiting += rootContainer.Dispose;
        root.ChildEnteredTree += OnInstallerNodeAdded;
    }

    private void OnInstallerNodeAdded(Node addedNode)
    {
        if (addedNode is not IInstallerNode installerNode)
        {
            return;
        }

        var container = Container.Create(installerNode.Path, installerNode.Install);
        installerNode.Free();
        addedNode.TreeExiting += container.Dispose;
    }
}
