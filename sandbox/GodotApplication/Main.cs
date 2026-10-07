using Godot;

public partial class Main : Node2D
{
    private Node _child;

    public override void _Ready()
    {
        GetTree().CreateTimer(1).Timeout += AddChildScene;
        GetTree().CreateTimer(2).Timeout += RemoveChildScene;
        GetTree().CreateTimer(3).Timeout += () => GetTree().Quit();
    }

    private void AddChildScene()
    {
        var childScene = ResourceLoader.Load<PackedScene>("res://Child.tscn");
        _child = childScene.Instantiate();
        AddChild(_child);
    }

    private void RemoveChildScene()
    {
        _child.QueueFree();
        GD.Print("Child removed.");
    }
}
