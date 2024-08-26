using DependencyInjection.Core;
using Godot;

namespace DependencyInjection.Godot;

public abstract partial class Installer : Node, IInstaller
{
    public abstract void Install(IContainerConfigurer containerConfigurer);
}
