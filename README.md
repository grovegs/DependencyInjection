# GroveGames.DependencyInjection

A lightweight dependency injection framework for .NET, Unity, and Godot with build-time validation and lifecycle phases. Built for games, where startup order and per-frame cost matter.

[![Build Status](https://github.com/grovegs/DependencyInjection/actions/workflows/release.yml/badge.svg)](https://github.com/grovegs/DependencyInjection/actions/workflows/release.yml)
[![Tests](https://github.com/grovegs/DependencyInjection/actions/workflows/tests.yml/badge.svg)](https://github.com/grovegs/DependencyInjection/actions/workflows/tests.yml)
[![Latest Release](https://img.shields.io/github/v/release/grovegs/DependencyInjection)](https://github.com/grovegs/DependencyInjection/releases/latest)
[![NuGet](https://img.shields.io/nuget/v/GroveGames.DependencyInjection)](https://www.nuget.org/packages/GroveGames.DependencyInjection)

---

## Features

- **Build-Time Validation**: Missing registrations, duplicate service types and circular dependencies fail when the container is built, not during gameplay
- **Lifecycle Phases**: Sync and async pre-initialize, initialize and post-initialize phases; Unity and Godot add per-frame updates
- **Automatic Detection**: Lifecycle interfaces and `IDisposable` are picked up from the implementation; there is nothing extra to register
- **Scoped Containers**: Child containers per scene that resolve from their parent and are disposed with the scene
- **Low Overhead**: Constructors are discovered once at build time and singleton resolves do not allocate
- **Native AOT Compatible**: Trimming and AOT analyzers enabled on .NET 8 and later
- **Unity Integration**: Available as a Unity package
- **Godot Integration**: Available as a Godot addon

## .NET

Install via NuGet:

```bash
dotnet add package GroveGames.DependencyInjection
```

### Registering Services

```csharp
using GroveGames.DependencyInjection;

var builder = new ContainerBuilder();
builder.AddSingleton<IAudioPlayer, AudioPlayer>();
builder.AddSingleton<GameLoop>();
builder.AddTransient<Enemy>();
builder.AddSingleton(settings);
builder.AddSingleton<IGateway>(resolver => new Gateway(resolver.Resolve<IClock>(), settings.Url));

using var root = builder.Build();
await root.InitializeAsync(cancellationToken);
```

Each registration binds exactly one service type: `AddSingleton<T>()` binds `T` and `AddSingleton<TService, TImplementation>()` binds `TService`. A singleton is one instance per container, so a child container can register its own. Value types work like any other service, for example `builder.AddSingleton(new GameModeConfig(...))` and `resolver.Resolve<GameModeConfig>()`. To expose one instance under another type, register an alias factory:

```csharp
builder.AddSingleton<ElephantAds>();
builder.AddSingleton<IAds>(resolver => resolver.Resolve<ElephantAds>());
```

A container never disposes or initializes an instance that it or a parent container already owns, so aliases are safe.

### Lifecycle

| Order | Interface                 | Method                                             |
| ----- | ------------------------- | -------------------------------------------------- |
| 1     | `IPreInitializable`       | `void PreInitialize()`                             |
| 2     | `IAsyncPreInitializable`  | `ValueTask PreInitializeAsync(CancellationToken)`  |
| 3     | `IInitializable`          | `void Initialize()`                                |
| 4     | `IAsyncInitializable`     | `ValueTask InitializeAsync(CancellationToken)`     |
| 5     | `IPostInitializable`      | `void PostInitialize()`                            |
| 6     | `IAsyncPostInitializable` | `ValueTask PostInitializeAsync(CancellationToken)` |

Each phase finishes before the next one starts: its sync methods run first, then its async methods are awaited one at a time. Entries run in registration order within a step. Every entry point is constructed, with its dependencies injected, before any phase runs, so a constructor only receives dependencies and must not use them. To use another service's initialized state, initialize in a later step than it: an `InitializeAsync` can rely on any `Initialize`, but an `Initialize` that needs an `InitializeAsync` result belongs in `PostInitialize`. Lifecycle interfaces are detected on singletons only.

### Child Containers

```csharp
using var scene = root.CreateChild(builder => builder.AddSingleton<Level>());
await scene.InitializeAsync(cancellationToken);
```

Disposing a container cancels a pending initialization, disposes its children and then disposes every instance it created in reverse creation order. Instances passed to `AddSingleton(instance)` are owned by the caller and are not disposed.

### Resolving All Implementations

`ResolveAll<T>()` returns every singleton of a container whose implementation is a `T`, in registration order, without instances owned by a parent container. The engine integrations use it to collect per-frame objects, and it works the same for your own interfaces:

```csharp
var tickables = container.ResolveAll<ITickable>();
```

### Method Injection

Objects the container does not construct, such as engine components, receive dependencies through `[Inject]` methods:

```csharp
public sealed class HealthBar
{
    [Inject]
    public void Construct(IPlayer player)
    {
    }
}

container.Inject(healthBar);
```

### Core Components

- **`ContainerBuilder`**: Collects registrations and builds a validated root container
- **`IContainer`**: Resolves services, runs lifecycle phases and owns created instances; `AddDisposable` hands any other cleanup to the container
- **`IObjectResolver`**: `Resolve`, `ResolveAll`, `Inject` and `AddDisposable`; it is what factories and services receive
- **`IInstaller`**: Groups registrations for a container or child container

## Unity

There are two installation steps required to use it in Unity.

1. Install `GroveGames.DependencyInjection` from NuGet using [NuGetForUnity](https://github.com/GlitchEnzo/NuGetForUnity). Open Window from NuGet → Manage NuGet Packages, search "GroveGames.DependencyInjection" and press Install.

2. Install the `GroveGames.DependencyInjection.Unity` package by referencing the git URL:

    ```text
    https://github.com/grovegs/DependencyInjection.git?path=src/GroveGames.DependencyInjection.Unity/Packages/com.grovegames.dependencyinjection
    ```

3. Create a `csc.rsp` file in your `Assets/` directory with the following content to enable C# 10 features:

    ```text
    -langversion:10
    -nullable:enable
    ```

### Root Installers

Derive from `RootInstaller` and list the assets in Edit → Project Settings → GroveGames → Dependency Injection. Settings are stored as a ScriptableObject in `Assets/Settings/DependencyInjectionSettings.asset` and added to preloaded assets.

```csharp
using GroveGames.DependencyInjection;
using GroveGames.DependencyInjection.Unity;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Root Installer")]
public sealed class GameRootInstaller : RootInstaller
{
    [SerializeField] private AudioOutput _audioOutputPrefab;

    public override void Install(IContainerBuilder builder)
    {
        builder.AddSingleton(resolver => resolver.Instantiate(_audioOutputPrefab));
        builder.AddSingleton<IAudioPlayer, AudioPlayer>();
    }
}
```

The root container is built before the first scene loads and initialized asynchronously.

### Scene Installers

Add components derived from `SceneInstaller` to an active root GameObject of a scene. In the first installer's `Awake`, which runs at execution order `int.MinValue` before other scripts, every active root GameObject of the scene is deactivated so no other `Awake`, `OnEnable` or `Start` runs yet. After the root finishes initializing, a child container of the root is built from every installer in the scene, `[Inject]` methods are called on every `MonoBehaviour` of the scene, and the container is fully initialized. Only then are the root GameObjects reactivated, so scene scripts start with their dependencies injected and every initializer complete. If initialization fails, the error is logged and the scene is started anyway. The container is disposed as soon as the scene's installers are destroyed, so nothing updates against destroyed objects while the scene unloads. Subclasses that override `Awake` or `OnDestroy` must call the base method. Scripts that another package or Project Settings → Script Execution Order places at `int.MinValue` too may run first; they are not injected and do not wait for initialization.

```csharp
public sealed class BattleInstaller : SceneInstaller
{
    [SerializeField] private BattleView _view;

    public override void Install(IContainerBuilder builder)
    {
        builder.AddSingleton(_view);
        builder.AddSingleton<BattleController>();
    }
}
```

Singletons implementing `IUpdatable`, `IFixedUpdatable` or `ILateUpdatable` from `GroveGames.DependencyInjection.Unity` are updated from the player loop once their container is initialized, and stop when it is disposed, so no `MonoBehaviour` lifecycle methods are needed.

#### Loading Scenes

Scene loaders do not need to know about containers, but a loader that should keep its loading screen up until the new scene is ready can wait for it. `WhenSceneReadyAsync` completes with `true` once the scene's container is initialized and its objects are reactivated, with `true` immediately for a loaded scene without installers, and with `false` if initialization failed or the scene was unloaded first.

```csharp
public async Task ChangeSceneAsync(string sceneName)
{
    var previous = SceneManager.GetActiveScene();
    await SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
    var scene = SceneManager.GetSceneByName(sceneName);

    await ContainerBootstrapper.WhenSceneReadyAsync(scene);

    SceneManager.SetActiveScene(scene);
    await SceneManager.UnloadSceneAsync(previous);
}
```

Objects created through `Instantiate` by a scene container, or by a container created from it, are placed in that container's scene, whichever scene is active.

#### Code Outside Containers

Scene objects and objects created through `Instantiate` are injected automatically. For objects created elsewhere, such as by another package's `Object.Instantiate`, look up the container of their scene and inject them, or resolve from it directly. `TryGetContainer` returns the scene's container, or the root container for a scene without installers, and only once that container is initialized.

```csharp
if (ContainerBootstrapper.TryGetContainer(instance.scene, out var container))
{
    container!.InjectGameObject(instance);
}
```

Code without a scene, such as a `ScriptableObject`, can use `ContainerBootstrapper.Root`.

### Unity Components

Unity types live in the `GroveGames.DependencyInjection.Unity` namespace.

- **`RootInstaller`**: ScriptableObject installer for the root container
- **`SceneInstaller`**: MonoBehaviour installer for a scene container
- **`ContainerBootstrapper`**: Builds the root and scene containers and exposes `Root`, `TryGetContainer` and `WhenSceneReadyAsync`
- **`DependencyInjectionSettings`**: ScriptableObject listing the root installers
- **`Instantiate`**: Creates a component from a prefab or on a new GameObject and injects `[Inject]` methods on every `MonoBehaviour` of it before `Awake` runs. The container that created it owns the GameObject and destroys it when disposed, after the component itself. Without a parent, it is placed in the scene of a scene container, or kept across scene loads for the root container
- **`InjectGameObject`**: Injects `[Inject]` methods on every `MonoBehaviour` of an existing hierarchy

## Godot

Install via NuGet:

```bash
dotnet add package GroveGames.DependencyInjection
```

Download the Godot addon from the [latest release](https://github.com/grovegs/DependencyInjection/releases/latest) and extract it to your project's `addons` folder. Enable the addon in Project Settings → Plugins, which registers the `ContainerBootstrapper` autoload.

```text
res://
├── addons/
│   └── GroveGames.DependencyInjection/
│       ├── plugin.cfg
│       └── ...
└── ...
```

### Root Installers

Derive from `RootInstaller`, create a resource for it and add it to a `DependencyInjectionSettingsResource`. The settings resource path is stored in project settings under `grove_games/dependency_injection/settings_resource`.

```csharp
using Godot;
using GroveGames.DependencyInjection;
using GroveGames.DependencyInjection.Godot;

[GlobalClass]
public partial class GameRootInstaller : RootInstaller
{
    public override void Install(IContainerBuilder builder)
    {
        builder.AddSingleton<IAudioPlayer, AudioPlayer>();
    }
}
```

### Scene Installers

Add a node derived from `SceneInstaller` to a scene. A child container of the root is built when the node enters the tree and disposed when it exits.

```csharp
public sealed partial class MainInstaller : SceneInstaller
{
    [Export] private PlayerNode _player;

    public override void Install(IContainerBuilder builder)
    {
        builder.AddSingleton(_player);
    }
}
```

### Godot Components

Godot types live in the `GroveGames.DependencyInjection.Godot` namespace.

- **`RootInstaller`**: Resource installer for the root container
- **`SceneInstaller`**: Node installer for a scene container
- **`ContainerBootstrapper`**: Autoload that builds containers and calls `IProcessable.Process` and `IPhysicsProcessable.PhysicsProcess` on their singletons from `_Process` and `_PhysicsProcess`
- **`DependencyInjectionSettingsResource`**: Resource listing the root installers
- **`InjectTree`** and **`Instantiate`**: Inject `[Inject]` methods on a node tree

## Performance

Measured with BenchmarkDotNet on .NET 10 against Microsoft.Extensions.DependencyInjection (`sandbox/DotnetBenchmark`):

| Benchmark                 | GroveGames | Microsoft | Allocated (GroveGames / Microsoft) |
| ------------------------- | ---------- | --------- | ---------------------------------- |
| Singleton resolve         | 5.0 ns     | 3.2 ns    | 0 B / 0 B                          |
| Transient resolve, 3 deps | 32.8 ns    | 6.4 ns    | 24 B / 24 B                        |
| Factory resolve           | 9.6 ns     | 8.3 ns    | 24 B / 24 B                        |
| Build, 4 registrations    | 906 ns     | 783 ns    | 2736 B / 5664 B                    |

Constructors are invoked through reflection so the same code runs on IL2CPP, where runtime code generation is not available.

---

## Testing

Run tests:

```bash
dotnet test
```

---

## Contributing

Contributions are welcome! Please:

1. Fork the repository
2. Create a feature branch
3. Write tests for new functionality
4. Submit a pull request

---

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
