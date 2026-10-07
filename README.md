# DependencyInjection


[![Build Status](https://github.com/grovegs/DependencyInjection/actions/workflows/release.yml/badge.svg)](https://github.com/grovegs/DependencyInjection/actions/workflows/release.yml)
[![Tests](https://github.com/grovegs/DependencyInjection/actions/workflows/tests.yml/badge.svg)](https://github.com/grovegs/DependencyInjection/actions/workflows/tests.yml)
[![Latest Release](https://img.shields.io/github/v/release/grovegs/DependencyInjection)](https://github.com/grovegs/DependencyInjection/releases/latest)
[![NuGet](https://img.shields.io/nuget/v/GroveGames.DependencyInjection)](https://www.nuget.org/packages/GroveGames.DependencyInjection)

A lightweight dependency injection framework developed by Grove Games for .NET, Unity and Godot, with build-time validation and lifecycle phases.

## Usage

```csharp
var builder = new ContainerBuilder();
builder.AddSingleton<IAudioPlayer, AudioPlayer>();
builder.AddSingleton<GameLoop>();
builder.AddTransient<Enemy>();
builder.AddSingleton(settings);
builder.AddSingleton<IGateway>(resolver => new Gateway(resolver.Resolve<IClock>(), settings.Url));
builder.AddSingleton<ElephantAds>();
builder.AddSingleton<IAds>(resolver => resolver.Resolve<ElephantAds>());

var root = builder.Build();
await root.InitializeAsync(cancellationToken);

var scene = root.CreateChild(new SceneInstaller());
await scene.InitializeAsync(cancellationToken);

root.Update(deltaTime);
scene.Dispose();
```

`Build()` resolves every constructor once and links dependencies directly, so missing registrations, duplicate service types and circular dependencies fail at build time instead of during gameplay.

Each registration binds exactly one service type: `AddSingleton<T>()` binds `T`, `AddSingleton<TService, TImplementation>()` binds `TService`. To expose one instance under another type, register an alias factory that resolves it. A container never disposes or initializes an instance that it or a parent container already owns, so aliases are safe.

### Lifecycle

Lifecycle interfaces and `IDisposable` are detected automatically on singletons; they never need to be registered.

| Order | Interface | Method |
|---|---|---|
| 1 | `IAsyncPreInitializable` | `ValueTask PreInitializeAsync(CancellationToken)` |
| 2 | `IAsyncInitializable` | `ValueTask InitializeAsync(CancellationToken)` |
| 3 | `IAsyncPostInitializable` | `ValueTask PostInitializeAsync(CancellationToken)` |
| 4 | `IPreInitializable` | `void PreInitialize()` |
| 5 | `IInitializable` | `void Initialize()` |
| 6 | `IPostInitializable` | `void PostInitialize()` |
| every frame | `IUpdatable`, `IFixedUpdatable`, `ILateUpdatable` | `Update(float)`, `FixedUpdate(float)`, `LateUpdate(float)` |

Every async phase is awaited before any sync phase runs. Within a phase, entries run in registration order. Updating a container also updates its children. Disposing a container cancels a pending initialization, disposes its children and then disposes every instance it created in reverse creation order. Instances passed to `AddSingleton(object)` are not disposed by the container.

### Unity

- Create root installers by deriving from `RootInstaller` (a `ScriptableObject`) and list them in **Project Settings > GroveGames > Dependency Injection**.
- Add `SceneInstaller` components to a scene to get a child container for that scene. It is created when the scene loads, after the root finishes initializing, and disposed when the scene unloads.
- Updates are driven from the player loop; no `MonoBehaviour` lifecycle methods are used.
- `AddSingletonFromPrefab`, `AddSingletonOnNewGameObject` and `resolver.Instantiate` inject `[Inject]` methods on every `MonoBehaviour` of the created object.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
