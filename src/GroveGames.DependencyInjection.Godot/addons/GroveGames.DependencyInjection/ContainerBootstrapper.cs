using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Godot;

namespace GroveGames.DependencyInjection.Godot;

public sealed partial class ContainerBootstrapper : Node
{
    private static readonly Dictionary<Node, SceneState> s_scenes = new();
    private static IContainer s_root;
    private static ulong s_lastProcessTicks;
    private static float s_unscaledFrameTime;
    private static Task s_rootInitialization;

    private PostFrameUpdater _postFrameUpdater;

    public ContainerBootstrapper()
    {
        ProcessPriority = int.MinValue;
        ProcessPhysicsPriority = int.MinValue;
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
        if (_postFrameUpdater == null)
        {
            _postFrameUpdater = new PostFrameUpdater();
            AddChild(_postFrameUpdater);
        }
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

        foreach (var state in s_scenes.Values)
        {
            state.Ready.TrySetResult(false);
        }

        s_scenes.Clear();
        ContainerRegistry.Clear();
        s_rootInitialization = null;
        var root = s_root;
        s_root = null;
        s_lastProcessTicks = 0;

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
        var root = s_root;

        if (root == null)
        {
            return;
        }

        s_unscaledFrameTime = GetUnscaledFrameTime(delta);
        var deltaTime = new DeltaTime((float)delta, s_unscaledFrameTime);

        try
        {
            root.PreFrameUpdate(deltaTime);
        }
        catch (Exception exception)
        {
            PushError(exception);
        }

        try
        {
            root.FrameUpdate(deltaTime);
        }
        catch (Exception exception)
        {
            PushError(exception);
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        var root = s_root;

        if (root == null)
        {
            return;
        }

        try
        {
            root.PhysicsUpdate(new DeltaTime((float)delta, 1f / Engine.PhysicsTicksPerSecond));
        }
        catch (Exception exception)
        {
            PushError(exception);
        }
    }

    private static void PostFrameUpdate(DeltaTime deltaTime)
    {
        var root = s_root;

        if (root == null)
        {
            return;
        }

        try
        {
            root.PostFrameUpdate(deltaTime);
        }
        catch (Exception exception)
        {
            PushError(exception);
        }
    }

    private static float GetUnscaledFrameTime(double delta)
    {
        var now = Time.GetTicksUsec();
        var last = s_lastProcessTicks;
        s_lastProcessTicks = now;
        var timeScale = Engine.TimeScale;

        if (timeScale > 0)
        {
            return (float)(delta / timeScale);
        }

        if (last == 0)
        {
            return 0f;
        }

        var maximum = (float)Engine.MaxPhysicsStepsPerFrame / Engine.PhysicsTicksPerSecond;
        return Math.Min((now - last) / 1_000_000f, maximum);
    }

    private static void PushError(Exception exception)
    {
        if (exception is not AggregateException aggregate)
        {
            GD.PushError(exception.ToString());
            return;
        }

        var flattened = aggregate.Flatten();

        for (var i = 0; i < flattened.InnerExceptions.Count; i++)
        {
            GD.PushError(flattened.InnerExceptions[i].ToString());
        }
    }

    private static async Task InitializeRootAsync(IContainer root)
    {
        try
        {
            await root.InitializeAsync();

            if (ReferenceEquals(s_root, root))
            {
                ContainerRegistry.SetRoot(root);
            }
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
        ContainerRegistry.AddScene(scope);
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

            ContainerRegistry.SetSceneContainer(state.Scope, state.Container);
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
        ContainerRegistry.RemoveScene(state.Scope);

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
        private readonly Node _scope;
        private readonly ProcessModeEnum _processMode;
        private readonly TaskCompletionSource<bool> _ready;

        public Node Scope => _scope;
        public ProcessModeEnum ProcessMode => _processMode;
        public TaskCompletionSource<bool> Ready => _ready;
        public IContainer Container;
        public Action Exiting;

        public SceneState(Node scope)
        {
            _scope = scope;
            _processMode = scope.ProcessMode;
            _ready = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
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
            PostFrameUpdate(new DeltaTime((float)delta, s_unscaledFrameTime));
        }
    }
}
