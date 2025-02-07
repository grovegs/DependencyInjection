using Godot;

namespace GroveGames.DependencyInjection;

public sealed class AsyncSceneSwitcher
{
    private readonly SceneTree _sceneTree;
    private readonly string _scenePath;
    private readonly double _minimumDuration;
    private readonly Action? _onSceneReady;
    private double _elapsedTime;
    private ulong _lastFrameTime;

    public AsyncSceneSwitcher(SceneTree sceneTree, string scenePath, double minDuration, Action? onSceneReady = null)
    {
        _sceneTree = sceneTree;
        _scenePath = scenePath;
        _minimumDuration = minDuration;
        _onSceneReady = onSceneReady;
    }

    public void RequestSceneSwitch()
    {
        var error = ResourceLoader.LoadThreadedRequest(_scenePath);

        if (error != Error.Ok)
        {
            throw new SceneRequestException($"Failed to request scene switch for: {_scenePath}", error);
        }

        _elapsedTime = 0;
        _lastFrameTime = Time.GetTicksUsec();
        _sceneTree.ProcessFrame += OnProcessFrame;
    }

    private void SwitchScene(PackedScene packedScene)
    {
        var scene = packedScene.Instantiate();
        SceneInstaller.Install(scene, _sceneTree.Root);

        scene.Ready += () =>
        {
            _sceneTree.UnloadCurrentScene();
            _sceneTree.CurrentScene = scene;
            _onSceneReady?.Invoke();
        };

        _sceneTree.Root.AddChild(scene);
    }

    private void OnProcessFrame()
    {
        ulong currentTime = Time.GetTicksUsec();
        double delta = (currentTime - _lastFrameTime) / 1_000_000.0;
        _elapsedTime += delta;
        _lastFrameTime = currentTime;

        var status = ResourceLoader.LoadThreadedGetStatus(_scenePath);

        if (status == ResourceLoader.ThreadLoadStatus.Failed)
        {
            Cleanup();
            throw new SceneLoadException($"Failed to load scene: {_scenePath}");
        }

        if (_elapsedTime < _minimumDuration || status != ResourceLoader.ThreadLoadStatus.Loaded)
        {
            return;
        }

        if (ResourceLoader.LoadThreadedGet(_scenePath) is not PackedScene packedScene)
        {
            Cleanup();
            throw new SceneInstantiationException($"Failed to instantiate scene: {_scenePath}");
        }

        Cleanup();
        SwitchScene(packedScene);
    }

    private void Cleanup()
    {
        _sceneTree.ProcessFrame -= OnProcessFrame;
    }
}