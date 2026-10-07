using Godot;

namespace GroveGames.DependencyInjection.Godot;

public abstract partial class SceneInstaller : Node, IInstaller
{
    public abstract void Install(IContainerBuilder builder);
}
