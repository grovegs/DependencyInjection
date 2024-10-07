using Godot;

namespace DependencyInjection;

public abstract partial class RootInstaller : Node, IInstaller
{
    public abstract void Install(IContainerConfigurer containerConfigurer);
}
