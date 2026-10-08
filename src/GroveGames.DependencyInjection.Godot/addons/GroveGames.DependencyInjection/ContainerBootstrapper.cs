using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Godot;

namespace GroveGames.DependencyInjection.Godot;

public sealed partial class ContainerBootstrapper : Node
{
    private static readonly Dictionary<Node, SceneState> s_scenes = new();
    private static readonly List<ProcessEntry> s_processEntries = new();
    private static IContainer s_root;
    private static Task s_rootInitialization;

    public static IContainer Root => s_root;

    public ContainerBootstrapper()
    {
        ProcessPriority = int.MinValue;
        ProcessPhysicsPriority = int.MinValue;
    }

    public static bool TryGetContainer(Node node, out IContainer container)
    {
        container = null;

        if (node == null || !IsInstanceValid(node) || !node.IsInsideTree())
        {
            return false;
        }

        var state = FindScene(node);

        if (state != null)
        {
            if (!IsReady(state))
            {
                return false;
            }

            container = state.Container;
            return container != null;
        }

        var root = s_root;

        if (root == null || !root.IsInitialized)
        {
            return false;
        }

        container = root;
        return true;
    }

    public static Task<bool> WhenSceneReadyAsync(Node node)
    {
        if (node == null || !IsInstanceValid(node))
        {
            return Task.FromResult(false);
        }

        var state = FindScene(node);

        if (state != null)
        {
            return state.Ready.Task;
        }

        return Task.FromResult(node.IsInsideTree());
    }

    public override void _EnterTree()
    {
        AddChild(new PostFrameUpdater());
        var builder = new ContainerBuilder();
        var installer = DependencyInjectionSettingsResource.GetOrCreate().RootInstaller;

        try
        {
            if (installer != null)
            {
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

        foreach (var state in s_scenes.Values)
        {
            state.Ready.TrySetResult(false);
        }

        s_scenes.Clear();
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
        var deltaTime = (float)delta;

        for (var i = 0; i < s_processEntries.Count; i++)
        {
            var entry = s_processEntries[i];

            for (var j = 0; j < entry.PreFrameUpdatables.Count && !entry.Container.IsDisposed; j++)
            {
                try
                {
                    entry.PreFrameUpdatables[j].PreFrameUpdate(deltaTime);
                }
                catch (Exception exception)
                {
                    GD.PushError(exception.ToString());
                }
            }
        }

        for (var i = 0; i < s_processEntries.Count; i++)
        {
            var entry = s_processEntries[i];

            for (var j = 0; j < entry.FrameUpdatables.Count && !entry.Container.IsDisposed; j++)
            {
                try
                {
                    entry.FrameUpdatables[j].FrameUpdate(deltaTime);
                }
                catch (Exception exception)
                {
                    GD.PushError(exception.ToString());
                }
            }
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        var deltaTime = (float)delta;

        for (var i = 0; i < s_processEntries.Count; i++)
        {
            var entry = s_processEntries[i];

            for (var j = 0; j < entry.PhysicsUpdatables.Count && !entry.Container.IsDisposed; j++)
            {
                try
                {
                    entry.PhysicsUpdatables[j].PhysicsUpdate(deltaTime);
                }
                catch (Exception exception)
                {
                    GD.PushError(exception.ToString());
                }
            }
        }
    }

    private static void PostFrameUpdate(float deltaTime)
    {
        for (var i = 0; i < s_processEntries.Count; i++)
        {
            var entry = s_processEntries[i];

            for (var j = 0; j < entry.PostFrameUpdatables.Count && !entry.Container.IsDisposed; j++)
            {
                try
                {
                    entry.PostFrameUpdatables[j].PostFrameUpdate(deltaTime);
                }
                catch (Exception exception)
                {
                    GD.PushError(exception.ToString());
                }
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
        if (node is not SceneInstaller installer)
        {
            return;
        }

        var scope = GetScope(installer);

        if (s_scenes.ContainsKey(scope))
        {
            return;
        }

        var state = new SceneState(scope);
        state.Exiting = () => DisposeScene(state);
        scope.TreeExiting += state.Exiting;
        s_scenes.Add(scope, state);
        scope.ProcessMode = ProcessModeEnum.Disabled;
        _ = InitializeSceneAsync(state);
    }

    private static async Task InitializeSceneAsync(SceneState state)
    {
        var root = s_root;
        var rootInitialization = s_rootInitialization;

        if (root == null || root.IsDisposed || rootInitialization == null)
        {
            GD.PushError($"Scene '{state.Scope.Name}' was started without its container because the root container is not available.");
            Complete(state, false);
            return;
        }

        try
        {
            var installers = new List<SceneInstaller>();
            var nestedScopes = new HashSet<Node>();
            CollectInstallers(state.Scope, state.Scope, installers, nestedScopes);
            nestedScopes.Remove(state.Scope);
            var container = root.CreateChild(builder =>
            {
                for (var i = 0; i < installers.Count; i++)
                {
                    installers[i].Install(builder);
                }
            });

            state.Container = container;
            InjectScope(container, state.Scope, nestedScopes);
        }
        catch (Exception exception)
        {
            GD.PushError(exception.ToString());
            GD.PushError($"Scene '{state.Scope.Name}' was started without its container.");
            Complete(state, false);
            return;
        }

        try
        {
            await rootInitialization;
        }
        catch
        {
            if (IsCurrent(state))
            {
                GD.PushError($"Scene '{state.Scope.Name}' was started without its container because the root container failed to initialize.");
                Complete(state, false);
            }

            return;
        }

        if (!IsCurrent(state))
        {
            return;
        }

        try
        {
            await state.Container.InitializeAsync();

            if (!IsCurrent(state))
            {
                return;
            }

            AddProcessEntry(state.Container);
            Complete(state, true);
        }
        catch (Exception exception)
        {
            if (!IsCurrent(state))
            {
                return;
            }

            GD.PushError(exception.ToString());
            GD.PushError($"Scene '{state.Scope.Name}' was started without a fully initialized container.");
            Complete(state, false);
        }
    }

    private static void DisposeScene(SceneState state)
    {
        if (!IsCurrent(state))
        {
            return;
        }

        s_scenes.Remove(state.Scope);

        if (IsInstanceValid(state.Scope))
        {
            state.Scope.TreeExiting -= state.Exiting;
            state.Scope.ProcessMode = state.ProcessMode;
        }

        state.Ready.TrySetResult(false);
        var container = state.Container;

        if (container == null)
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

    private static void Complete(SceneState state, bool isReady)
    {
        if (IsInstanceValid(state.Scope))
        {
            state.Scope.ProcessMode = state.ProcessMode;
        }

        state.Ready.TrySetResult(isReady);
    }

    private static bool IsCurrent(SceneState state)
    {
        return s_scenes.TryGetValue(state.Scope, out var current) && ReferenceEquals(current, state);
    }

    private static bool IsReady(SceneState state)
    {
        var ready = state.Ready.Task;
        return ready.IsCompleted && ready.Result;
    }

    private static SceneState FindScene(Node node)
    {
        for (var current = node; current != null; current = current.GetParent())
        {
            if (s_scenes.TryGetValue(current, out var state))
            {
                return state;
            }
        }

        return null;
    }

    private static Node GetScope(SceneInstaller installer)
    {
        return installer.Owner ?? installer;
    }

    private static void CollectInstallers(Node node, Node scope, List<SceneInstaller> installers, HashSet<Node> nestedScopes)
    {
        if (node is SceneInstaller installer)
        {
            var installerScope = GetScope(installer);

            if (installerScope == scope)
            {
                installers.Add(installer);
            }
            else
            {
                nestedScopes.Add(installerScope);
            }
        }

        var children = node.GetChildren(true);

        for (var i = 0; i < children.Count; i++)
        {
            CollectInstallers(children[i], scope, installers, nestedScopes);
        }
    }

    private static void InjectScope(IContainer container, Node node, HashSet<Node> nestedScopes)
    {
        if (nestedScopes.Contains(node))
        {
            return;
        }

        container.Inject(node);
        var children = node.GetChildren(true);

        for (var i = 0; i < children.Count; i++)
        {
            InjectScope(container, children[i], nestedScopes);
        }
    }

    private sealed class SceneState
    {
        public readonly Node Scope;
        public readonly ProcessModeEnum ProcessMode;
        public readonly TaskCompletionSource<bool> Ready;
        public IContainer Container;
        public Action Exiting;

        public SceneState(Node scope)
        {
            Scope = scope;
            ProcessMode = scope.ProcessMode;
            Ready = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        }
    }

    private sealed class ProcessEntry
    {
        public readonly IContainer Container;
        public readonly IReadOnlyList<IPreFrameUpdatable> PreFrameUpdatables;
        public readonly IReadOnlyList<IFrameUpdatable> FrameUpdatables;
        public readonly IReadOnlyList<IPostFrameUpdatable> PostFrameUpdatables;
        public readonly IReadOnlyList<IPhysicsUpdatable> PhysicsUpdatables;

        public ProcessEntry(IContainer container)
        {
            Container = container;
            PreFrameUpdatables = container.ResolveAll<IPreFrameUpdatable>();
            FrameUpdatables = container.ResolveAll<IFrameUpdatable>();
            PostFrameUpdatables = container.ResolveAll<IPostFrameUpdatable>();
            PhysicsUpdatables = container.ResolveAll<IPhysicsUpdatable>();
        }
    }

    private sealed partial class PostFrameUpdater : Node
    {
        public PostFrameUpdater()
        {
            ProcessPriority = int.MaxValue;
        }

        public override void _Process(double delta)
        {
            PostFrameUpdate((float)delta);
        }
    }
}
