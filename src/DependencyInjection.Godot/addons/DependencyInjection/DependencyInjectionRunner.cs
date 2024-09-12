using Godot;

namespace DependencyInjection;

public sealed partial class DependencyInjectionRunner : Node
{
    public override void _Ready()
    {
        var installerScene = ResourceLoader.Load<PackedScene>("res://ApplicationInstaller.tscn");
        var installer = installerScene.Instantiate<Installer>();
        var container = DI.CreateContainer("root", new NullContainer(), installer.Install);
        installer.Free();
        var root = GetTree().Root;
        root.TreeExiting += () => DI.DisposeContainer(container);
        root.ChildEnteredTree += OnSceneAdded;
    }

    private void OnSceneAdded(Node node)
    {
        if (node.GetParent() != GetTree().Root)
        {
            return;
        }

        if (node is not Installer installer)
        {
            return;
        }

        var root = DI.FindContainer("root");

        if (root != null)
        {
            var container = root.AddChild("scene", installer);
            installer.Free();
            node.TreeExiting += () => DI.DisposeContainer(container);
        }

        installer.Free();
    }
}
