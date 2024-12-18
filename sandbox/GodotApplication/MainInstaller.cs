using Godot;

using GroveGames.DependencyInjection;

using Container = GroveGames.DependencyInjection.Container;

public sealed partial class MainInstaller : SceneInstallerNode
{
    [Export] private NodeExample _instance;

    public override void Install(IContainerConfigurer containerConfigurer)
    {
        GD.Print("Main installed.");
        containerConfigurer.AddSingleton(typeof(ISingleton), typeof(Singleton));
        containerConfigurer.AddSingleton<INodeExample>(_instance);
        containerConfigurer.AddSingleton(typeof(InitializeExample));
    }
}

public interface ISingleton
{
}

public sealed class Singleton : ISingleton
{
    public Singleton()
    {
        GD.Print("Singleton injected.");
    }
}

public partial class NodeExample : Node, INodeExample
{
}

public interface INodeExample
{
}

public class InitializeExample
{
    [Inject]
    public void Construct(INodeExample nodeExample, ISingleton singleton)
    {
        var container = Container.Find("/main");
        container.AddChild("world", (IContainerConfigurer c) =>
        {
            c.AddSingleton(typeof(ISingleton), typeof(Singleton));
        });

        var world = Container.Find("/main/world");
        world.Dispose();
    }
}