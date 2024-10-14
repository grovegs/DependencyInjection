using Godot;

namespace GroveGames.DependencyInjection;

public abstract partial class InstallerBase : Node, IInstaller
{
    [Export] private string _path = string.Empty;

    public string Path => _path;

    public abstract void Install(IContainerConfigurer containerConfigurer);
}