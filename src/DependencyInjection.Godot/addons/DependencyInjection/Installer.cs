using Godot;

namespace DependencyInjection;

public abstract partial class Installer : Node, IInstaller
{
    public abstract void Install(IContainerConfigurer containerConfigurer);
}
