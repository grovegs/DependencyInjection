using System;
using System.Collections.Generic;
using DependencyInjection.Core;
using Godot;

namespace DependencyInjection.Godot;

public sealed partial class ApplicationInitializer : Node
{
    private Dictionary<ReadOnlyMemory<char>, IContainer> _keyValuePairs;

    public override void _EnterTree()
    {
        var containerBuilder = new ContainerBuilder(Containers.Root);
        var installerScene = ResourceLoader.Load<PackedScene>("res://ApplicationInstaller.tscn");
        var installer = installerScene.Instantiate<Installer>();
        installer.Install(containerBuilder);
        Containers.Application = containerBuilder.Build();
        installer.Free();
        GetTree().Root.ChildEnteredTree += OnSceneAdded;
        GetTree().Root.ChildExitingTree += OnSceneRemoved;
    }

    public override void _ExitTree()
    {
        var applicationContainer = Containers.Application;
        applicationContainer?.Dispose();
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

        var containerBuilder = new ContainerBuilder(Containers.Application!);
        installer.Install(containerBuilder);
        Containers.Scene = containerBuilder.Build();
        installer.Free();
        //TODO: Find installer and create container with installer. Free installer.
    }

    private void OnSceneRemoved(Node node)
    {
        //TODO: Find depended container and dispose.
    }

    public override void _Notification(int what)
    {
    }
}
