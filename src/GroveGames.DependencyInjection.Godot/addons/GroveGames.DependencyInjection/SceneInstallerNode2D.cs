using System;
using Godot;

namespace GroveGames.DependencyInjection;

public abstract partial class SceneInstallerNode2D : Node2D, ISceneInstaller
{
    public abstract void Install(IContainerConfigurer containerConfigurer);
}
