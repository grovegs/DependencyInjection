# GroveGames.DependencyInjection

A lightweight dependency injection framework for .NET, Unity, and Godot with build-time validation and lifecycle phases. Built for games, where startup order and per-frame cost matter.

[![Build Status](https://github.com/grovegs/DependencyInjection/actions/workflows/release.yml/badge.svg)](https://github.com/grovegs/DependencyInjection/actions/workflows/release.yml)
[![Tests](https://github.com/grovegs/DependencyInjection/actions/workflows/tests.yml/badge.svg)](https://github.com/grovegs/DependencyInjection/actions/workflows/tests.yml)
[![Latest Release](https://img.shields.io/github/v/release/grovegs/DependencyInjection)](https://github.com/grovegs/DependencyInjection/releases/latest)
[![NuGet](https://img.shields.io/nuget/v/GroveGames.DependencyInjection)](https://www.nuget.org/packages/GroveGames.DependencyInjection)

---

## Features

- **Build-Time Validation**: Missing registrations, duplicate service types and circular dependencies fail when the container is built, not during gameplay
- **Lifecycle Phases**: Async and sync pre-initialize, initialize and post-initialize phases, plus update, fixed update and late update
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

Each registration binds exactly one service type: `AddSingleton<T>()` binds `T` and `AddSingleton<TService, TImplementation>()` binds `TService`. To expose one instance under another type, register an alias factory:

```csharp
builder.AddSingleton<ElephantAds>();
builder.AddSingleton<IAds>(resolver => resolver.Resolve<ElephantAds>());
```

A container never disposes or initializes an instance that it or a parent container already owns, so aliases are safe.

### Lifecycle

| Order       | Interface                                         | Method                                                     |
| ----------- | ------------------------------------------------- | ---------------------------------------------------------- |
| 1           | `IAsyncPreInitializable`                          | `ValueTask PreInitializeAsync(CancellationToken)`          |
| 2           | `IAsyncInitializable`                             | `ValueTask InitializeAsync(CancellationToken)`             |
| 3           | `IAsyncPostInitializable`                         | `ValueTask PostInitializeAsync(CancellationToken)`         |
| 4           | `IPreInitializable`                               | `void PreInitialize()`                                     |
| 5           | `IInitializable`                                  | `void Initialize()`                                        |
| 6           | `IPostInitializable`                              | `void PostInitialize()`                                    |
| every frame | `IUpdatable`, `IFixedUpdatable`, `ILateUpdatable` | `Update(float)`, `FixedUpdate(float)`, `LateUpdate(float)` |

Every async phase is awaited before any sync phase runs, and entries run in registration order within a phase. Lifecycle interfaces are detected on singletons only.

### Child Containers

```csharp
using var scene = root.CreateChild(builder => builder.AddSingleton<Level>());
await scene.InitializeAsync(cancellationToken);

root.Update(deltaTime);
```

Updating a container also updates its children. Disposing a container cancels a pending initialization, disposes its children and then disposes every instance it created in reverse creation order. Instances passed to `AddSingleton(instance)` are owned by the caller and are not disposed.

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
- **`IContainer`**: Resolves services, runs lifecycle phases, updates and owns created instances
- **`IObjectResolver`**: Resolve and inject API that can be injected into factories
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
        builder.AddSingletonFromPrefab(_audioOutputPrefab, dontDestroyOnLoad: true);
        builder.AddSingleton<IAudioPlayer, AudioPlayer>();
    }
}
```

The root container is built before the first scene loads and initialized asynchronously.

### Scene Installers

Add components derived from `SceneInstaller` to a scene. When the scene loads, a child container of the root is built from every installer in it, after the root finishes initializing. It is disposed when the scene unloads.

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

Updates are driven from the player loop, so no `MonoBehaviour` lifecycle methods are needed.

### Unity Components

- **`RootInstaller`**: ScriptableObject installer for the root container
- **`SceneInstaller`**: MonoBehaviour installer for a scene container
- **`ContainerBootstrapper`**: Builds the root and scene containers and exposes `Root` and `TryGetSceneContainer`
- **`DependencyInjectionSettings`**: ScriptableObject listing the root installers
- **`AddSingletonFromPrefab`** and **`AddSingletonOnNewGameObject`**: Register components created on first resolve
- **`InjectGameObject`** and **`Instantiate`**: Inject `[Inject]` methods on every `MonoBehaviour` of a hierarchy

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

- **`RootInstaller`**: Resource installer for the root container
- **`SceneInstaller`**: Node installer for a scene container
- **`ContainerBootstrapper`**: Autoload that builds containers and drives `_Process` and `_PhysicsProcess` updates
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
