using Godot;

public sealed class Singleton : ISingleton
{
    public Singleton()
    {
        GD.Print("Singleton injected.");
    }
}
