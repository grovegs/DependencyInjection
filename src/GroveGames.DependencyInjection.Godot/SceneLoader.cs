using Godot;

namespace GroveGames.DependencyInjection;

public sealed class SceneLoader
{
    private readonly SceneTree _sceneTree;
    private readonly string _scenePath;
    private readonly double _minimumDuration;
    private readonly Action? _onLoaded;
    private double _elapsedTime;
    private ulong _lastFrameTime;

    public SceneLoader(SceneTree sceneTree, string scenePath, double minDuration, Action? onLoaded = null)
    {
        _sceneTree = sceneTree;
        _scenePath = scenePath;
        _minimumDuration = minDuration;
        _onLoaded = onLoaded;
    }

    public void LoadRequest()
    {
        var error = ResourceLoader.LoadThreadedRequest(_scenePath);

        if (error != Error.Ok)
        {
            throw new SceneLoaderException($"Failed to load scene: {_scenePath}");
        }

        _elapsedTime = 0;
        _lastFrameTime = Time.GetTicksUsec();
        _sceneTree.ProcessFrame += OnProcessFrame;
    }

    private void Load(PackedScene packedScene)
    {
        var scene = packedScene.Instantiate();
        SceneInstaller.Install(scene, _sceneTree.Root);

        scene.Ready += () =>
        {
            _sceneTree.UnloadCurrentScene();
            _sceneTree.CurrentScene = scene;
            _onLoaded?.Invoke();
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
            throw new SceneLoaderException($"Failed to load scene: {_scenePath}");
        }

        if (_elapsedTime < _minimumDuration || status != ResourceLoader.ThreadLoadStatus.Loaded)
        {
            return;
        }

        if (ResourceLoader.LoadThreadedGet(_scenePath) is not PackedScene packedScene)
        {
            Cleanup();
            throw new SceneLoaderException($"Failed to instantiate loaded scene: {_scenePath}");
        }

        Cleanup();
        Load(packedScene);
    }

    private void Cleanup()
    {
        _sceneTree.ProcessFrame -= OnProcessFrame;
    }
}