using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Godot;

namespace GroveGames.DependencyInjection.Godot;

public sealed partial class ContainerBootstrapper : Node
{
    private static readonly Dictionary<SceneInstaller, IContainer> s_sceneContainers = new();
    private static readonly HashSet<SceneInstaller> s_pendingInstallers = new();
    private static readonly List<ProcessEntry> s_processEntries = new();
    private static IContainer s_root;
    private static Task s_rootInitialization;

    public static IContainer Root => s_root;

    public ContainerBootstrapper()
    {
        ProcessPriority = int.MinValue;
        ProcessPhysicsPriority = int.MinValue;
    }

    public static bool TryGetSceneContainer(SceneInstaller installer, out IContainer container)
    {
        return s_sceneContainers.TryGetValue(installer, out container);
    }

    public override void _EnterTree()
    {
        var builder = new ContainerBuilder();
        var installers = DependencyInjectionSettingsResource.GetOrCreate().RootInstallers;

        try
        {
            for (var i = 0; i < installers.Count; i++)
            {
                var installer = installers[i];

                if (installer == null)
                {
                    GD.PushError($"Root installer at index {i} is missing.");
                    continue;
                }

                installer.Install(builder);
            }

            s_root = builder.Build();
        }
        catch (Exception exception)
        {
            GD.PushError(exception.ToString());
            return;
        }

        GetTree().NodeAdded += OnNodeAdded;
        s_rootInitialization = InitializeRootAsync(s_root);
    }

    public override void _ExitTree()
    {
        GetTree().NodeAdded -= OnNodeAdded;
        s_processEntries.Clear();
        s_sceneContainers.Clear();
        s_pendingInstallers.Clear();
        s_rootInitialization = null;
        var root = s_root;
        s_root = null;

        if (root == null)
        {
            return;
        }

        try
        {
            root.Dispose();
        }
        catch (Exception exception)
        {
            GD.PushError(exception.ToString());
        }
    }

    public override void _Process(double delta)
    {
        for (var i = 0; i < s_processEntries.Count; i++)
        {
            var entry = s_processEntries[i];

            try
            {
                for (var j = 0; j < entry.Processables.Count && !entry.Container.IsDisposed; j++)
                {
                    entry.Processables[j].Process(delta);
                }
            }
            catch (Exception exception)
            {
                GD.PushError(exception.ToString());
            }
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        for (var i = 0; i < s_processEntries.Count; i++)
        {
            var entry = s_processEntries[i];

            try
            {
                for (var j = 0; j < entry.PhysicsProcessables.Count && !entry.Container.IsDisposed; j++)
                {
                    entry.PhysicsProcessables[j].PhysicsProcess(delta);
                }
            }
            catch (Exception exception)
            {
                GD.PushError(exception.ToString());
            }
        }
    }

    private static void AddProcessEntry(IContainer container)
    {
        if (container.IsDisposed)
        {
            return;
        }

        s_processEntries.Add(new ProcessEntry(container));
    }

    private static void RemoveProcessEntry(IContainer container)
    {
        for (var i = 0; i < s_processEntries.Count; i++)
        {
            if (ReferenceEquals(s_processEntries[i].Container, container))
            {
                s_processEntries.RemoveAt(i);
                return;
            }
        }
    }

    private static async Task InitializeRootAsync(IContainer root)
    {
        try
        {
            await root.InitializeAsync();
            AddProcessEntry(root);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception)
        {
            GD.PushError(exception.ToString());
            throw;
        }
    }

    private static void OnNodeAdded(Node node)
    {
        if (node is not SceneInstaller installer || s_sceneContainers.ContainsKey(installer) || !s_pendingInstallers.Add(installer))
        {
            return;
        }

        installer.TreeExiting += () => OnInstallerExiting(installer);
        _ = InitializeSceneAsync(installer);
    }

    private static async Task InitializeSceneAsync(SceneInstaller installer)
    {
        try
        {
            var rootInitialization = s_rootInitialization;

            if (rootInitialization == null)
            {
                s_pendingInstallers.Remove(installer);
                return;
            }

            await rootInitialization;
        }
        catch
        {
            s_pendingInstallers.Remove(installer);
            return;
        }

        var root = s_root;

        if (!s_pendingInstallers.Remove(installer) || root == null || root.IsDisposed || !IsInstanceValid(installer) || !installer.IsInsideTree())
        {
            return;
        }

        try
        {
            var container = root.CreateChild(installer);
            s_sceneContainers[installer] = container;
            await container.InitializeAsync();
            AddProcessEntry(container);
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception exception)
        {
            GD.PushError(exception.ToString());
        }
    }

    private static void OnInstallerExiting(SceneInstaller installer)
    {
        s_pendingInstallers.Remove(installer);

        if (!s_sceneContainers.Remove(installer, out var container))
        {
            return;
        }

        RemoveProcessEntry(container);

        try
        {
            container.Dispose();
        }
        catch (Exception exception)
        {
            GD.PushError(exception.ToString());
        }
    }

    private sealed class ProcessEntry
    {
        public readonly IContainer Container;
        public readonly IReadOnlyList<IProcessable> Processables;
        public readonly IReadOnlyList<IPhysicsProcessable> PhysicsProcessables;

        public ProcessEntry(IContainer container)
        {
            Container = container;
            Processables = container.ResolveAll<IProcessable>();
            PhysicsProcessables = container.ResolveAll<IPhysicsProcessable>();
        }
    }
}
