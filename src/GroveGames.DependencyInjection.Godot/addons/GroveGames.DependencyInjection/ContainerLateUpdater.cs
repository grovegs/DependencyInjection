using System;

using Godot;

namespace GroveGames.DependencyInjection;

internal sealed partial class ContainerLateUpdater : Node
{
    public ContainerLateUpdater()
    {
        Name = nameof(ContainerLateUpdater);
        ProcessPriority = int.MaxValue;
    }

    public override void _Process(double delta)
    {
        var root = ContainerBootstrapper.Root;

        if (root == null)
        {
            return;
        }

        try
        {
            root.LateUpdate((float)delta);
        }
        catch (Exception exception)
        {
            GD.PushError(exception.ToString());
        }
    }
}
