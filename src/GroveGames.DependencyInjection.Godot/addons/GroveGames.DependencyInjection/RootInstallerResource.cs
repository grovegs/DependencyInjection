using Godot;

namespace GroveGames.DependencyInjection;

public abstract partial class RootInstallerResource : Resource, IRootInstaller
{
    public abstract void Install(IContainerConfigurer containerConfigurer);
}
