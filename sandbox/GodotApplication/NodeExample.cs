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

    public override void _EnterTree()
    {
        GD.Print("NodeExample entered tree.");
    }

    public override void _Ready()
    {
        GD.Print("NodeExample ready.");
    }

    public override void _ExitTree()
    {
        GD.Print("NodeExample exited tree.");
    }
}
