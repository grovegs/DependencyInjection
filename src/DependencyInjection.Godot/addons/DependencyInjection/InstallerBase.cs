using Godot;

namespace DependencyInjection;

public abstract partial class InstallerBase : Node, IInstaller
{
    [Export] private string _path;

    public string Path => _path;

    public abstract void Install(IContainerConfigurer containerConfigurer);
}
