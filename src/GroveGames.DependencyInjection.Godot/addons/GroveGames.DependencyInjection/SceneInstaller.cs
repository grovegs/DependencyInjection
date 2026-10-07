using Godot;

namespace GroveGames.DependencyInjection;

public abstract partial class SceneInstaller : Node, IInstaller
{
    public abstract void Install(IContainerBuilder builder);
}
