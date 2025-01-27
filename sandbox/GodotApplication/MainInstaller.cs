using Godot;

using GroveGames.DependencyInjection;

public sealed partial class MainInstaller : SceneInstallerNode
{
    [Export] private NodeExample _instance;

    public override void Install(IContainerConfigurer containerConfigurer)
    {
        containerConfigurer.AddSingleton(typeof(ISingleton), typeof(Singleton));
        containerConfigurer.AddSingleton<INodeExample>(_instance);
        containerConfigurer.AddSingleton(typeof(InitializeExample));
        GD.Print("Main installed.");
    }
}
