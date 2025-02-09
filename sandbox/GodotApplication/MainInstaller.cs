using Godot;

using GroveGames.DependencyInjection;

public sealed partial class MainInstaller : SceneInstallerNode
{
    [Export] private NodeExample _instance;

    public override void Install(IContainerBuilder builder)
    {
        builder.AddSingleton(typeof(ISingleton), typeof(Singleton));
        builder.AddSingleton<INodeExample>(_instance);
        GD.Print("Main installed.");
    }
}
