using Godot;

using GroveGames.DependencyInjection;

public partial class NodeExample : Node, INodeExample
{
    private ISingleton _singleton;

    [Inject]
    public void Construct(ISingleton singleton)
    {
        _singleton = singleton;
        GD.Print("NodeExample injected.");
    }
}
