using DependencyInjection.Core;
using Godot;

namespace DependencyInjection.Godot;

public abstract partial class NodeInstaller : Node, IInstaller
{
    public abstract void Install(IContainerConfigurer containerConfigurer);
}
