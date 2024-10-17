using Godot;

namespace GroveGames.DependencyInjection;

public abstract partial class InstallerNode3D : Node3D, IInstallerNode
{
    [Export] private string _path;

    public ReadOnlySpan<char> Path => _path;

    public abstract void Install(IContainerConfigurer containerConfigurer);
}
