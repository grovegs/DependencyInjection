using Godot;

using GroveGames.DependencyInjection;
using GroveGames.DependencyInjection.Godot;

public sealed partial class MainInstaller : SceneInstaller
{
    [Export] private NodeExample _instance;

    public override void Install(IContainerBuilder builder)
    {
        builder.AddSingleton<INodeExample>(_instance);
        GD.Print("Main installed.");
    }
}
