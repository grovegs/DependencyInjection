﻿using DependencyInjection.Core;
using Godot;

namespace DependencyInjection.Godot;

public sealed partial class DependencyInjectionRunner : Node
{
    public override void _Ready()
    {
        var installerScene = ResourceLoader.Load<PackedScene>("res://ApplicationInstaller.tscn");
        var installer = installerScene.Instantiate<Installer>();
        var container = Container.Create("root", new NullContainer(), installer.Install);
        installer.Free();
        var root = GetTree().Root;
        root.TreeExiting += () => container?.Dispose();
        root.ChildEnteredTree += OnSceneAdded;
    }

    private void OnSceneAdded(Node node)
    {
        if (node.GetParent() != GetTree().Root)
        {
            return;
        }

        if (node is not Installer installer)
        {
            return;
        }

        var container = Container.Find("root")?.AddChild("scene", installer);
        installer.Free();
        node.TreeExiting += () => container?.Dispose();
    }
}