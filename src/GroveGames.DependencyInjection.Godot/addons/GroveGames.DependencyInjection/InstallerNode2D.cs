using Godot;

namespace GroveGames.DependencyInjection;

public abstract partial class InstallerNode2D : Node2D, IInstallerNode
{
    [Export] private string _path;

    public ReadOnlySpan<char> Path => _path;

    public abstract void Install(IContainerConfigurer containerConfigurer);
}
