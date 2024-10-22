using Godot;

namespace GroveGames.DependencyInjection;

public abstract partial class SceneInstallerNode3D : Node3D, ISceneInstaller
{
    public abstract void Install(IContainerConfigurer containerConfigurer);
}
