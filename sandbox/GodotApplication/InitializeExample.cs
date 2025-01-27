using Godot;

using GroveGames.DependencyInjection;

public class InitializeExample : IInitializable
{
    public InitializeExample(INodeExample nodeExample, ISingleton singleton)
    {
        var container = GroveGames.DependencyInjection.Container.Find("/main");
        container.AddChild("world", (IContainerConfigurer c) =>
        {
            c.AddSingleton(typeof(ISingleton), typeof(Singleton));
        });

        var world = GroveGames.DependencyInjection.Container.Find("/main/world");
        world.Dispose();
    }

    public void Initialize()
    {
        GD.Print("InitializeExample initialized.");
    }
}