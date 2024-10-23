using Godot;

public partial class Splash : Node2D
{
    public override void _Ready()
    {
        CallDeferred(nameof(ChangeScene));
    }

    private void ChangeScene()
    {
        GetTree().ChangeSceneToFile("res://Main.tscn");
    }
}