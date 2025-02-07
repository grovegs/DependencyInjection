using Godot;

namespace GroveGames.DependencyInjection;

public abstract partial class SceneInstallerNode : Node, ISceneInstaller
{
    public abstract void Install(IContainerBuilder builder);
}