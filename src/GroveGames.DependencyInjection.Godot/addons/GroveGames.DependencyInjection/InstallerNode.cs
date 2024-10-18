using System;
using Godot;

namespace GroveGames.DependencyInjection;

public abstract partial class InstallerNode : Node, IInstallerNode
{
    [Export] private string _path;

    public ReadOnlySpan<char> Path => _path;

    public abstract void Install(IContainerConfigurer containerConfigurer);
}
