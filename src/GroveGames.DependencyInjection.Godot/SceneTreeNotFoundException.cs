namespace GroveGames.DependencyInjection;

public class SceneTreeNotFoundException : Exception
{
    public SceneTreeNotFoundException() : base("SceneTree not found in main loop.") { }
}