using Godot;

public partial class Main : Node2D
{
    public override void _Ready()
    {
        var timer = new Timer
        {
            OneShot = true,
            WaitTime = 1,
        };
        AddChild(timer);
        timer.Timeout += () =>
        {
            var childScene = ResourceLoader.Load<PackedScene>("res://Child.tscn");
            var childInstance = childScene.Instantiate();
            AddChild(childInstance);
        };
        timer.Start();
    }
}