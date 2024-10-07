using Godot;

namespace DependencyInjection;

public abstract partial class RootInstallerBase : Node, IInstaller
{
    public abstract void Install(IContainerConfigurer containerConfigurer);
}
