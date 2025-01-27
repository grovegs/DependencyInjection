using Godot;

using GroveGames.DependencyInjection;

public partial class Splash : Node2D
{
    public override void _Ready()
    {
        CallDeferred(nameof(ChangeScene));
    }

    private void ChangeScene()
    {
        GetTree().ChangeScene("res://Main.tscn", 2);
    }
}