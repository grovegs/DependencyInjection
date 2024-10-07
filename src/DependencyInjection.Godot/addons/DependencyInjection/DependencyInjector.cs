﻿using Godot;

namespace DependencyInjection;

public sealed partial class DependencyInjector : Node
{
    public override void _Ready()
    {
        var rootInstallerPath = Settings.GetRootInstallerSetting();
        var rootInstallerScene = ResourceLoader.Load<PackedScene>(rootInstallerPath);
        var rootInstaller = rootInstallerScene.Instantiate<RootInstallerBase>();
        var rootContainer = RootContainer.Create(rootInstaller.Install);
        rootInstaller.Free();
        var root = GetTree().Root;
        root.TreeExiting += rootContainer.Dispose;
        root.ChildEnteredTree += OnInstallerAdded;
    }

    private void OnInstallerAdded(Node addedNode)
    {
        if (addedNode is not InstallerBase installer)
        {
            return;
        }

        var container = Container.Create(installer.Path, installer.Install);
        installer.Free();
        addedNode.TreeExiting += container.Dispose;
    }
}
