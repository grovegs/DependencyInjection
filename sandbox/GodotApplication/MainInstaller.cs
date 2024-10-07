using DependencyInjection;
using Godot;
using Container = DependencyInjection.Container;

public sealed partial class MainInstaller : InstallerBase
{
    [Export] private NodeExample _instance;

    public override void Install(IContainerConfigurer containerConfigurer)
    {
        containerConfigurer.AddSingleton(typeof(ISingleton), typeof(Singleton));
        containerConfigurer.AddInstance<INodeExample>(_instance);
        containerConfigurer.AddInstance(typeof(InitializeExample));
    }
}

public interface ISingleton
{
}

public sealed class Singleton : ISingleton, IInitializable
{
    public void Initialize()
    {
        GD.Print("Test");
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
        var container = Container.Find("/root/main");
        container.AddChild("world", (IContainerConfigurer c) =>
        {
            c.AddSingleton(typeof(ISingleton), typeof(Singleton));
        });

        var world = Container.Find("/root/main/world");
        world.Dispose();
    }
}
