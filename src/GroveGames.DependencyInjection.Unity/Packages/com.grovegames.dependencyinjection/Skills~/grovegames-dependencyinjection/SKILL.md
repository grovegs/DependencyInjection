---
name: grovegames-dependencyinjection
description: Rules for wiring game code with GroveGames.DependencyInjection, covering installers, registration order, the initialize phases, injecting MonoBehaviours and disposal. Use when adding or changing an installer, registering a service, implementing IInitializable or an update interface, or debugging startup or shutdown order.
---

# Using GroveGames.DependencyInjection

Rules for game code that is wired with this package. They hold in every game. The API is in the package's `README.md`.

## Registration

- Register with `AddSingleton<IX, X>()`, `AddSingleton(instance)`, or a factory lambda for arguments the container cannot inject.
- Registration order is execution order.

## Lifecycle

The container builds every object first, then runs the phases below in order. A phase finishes for every object before the next starts. An async phase awaits each object before calling the next one.

| Phase                 | Do                                                               | Do not                                               |
| --------------------- | ---------------------------------------------------------------- | ---------------------------------------------------- |
| Constructor           | Store dependencies, create collections and delegates             | Call a dependency. It exists but has not initialized |
| `PreInitialize`       | Load and validate saved state and definitions                    | Raise events or read another service's state         |
| `PreInitializeAsync`  | Load own state that has to be awaited                            | Raise events or read another service's state         |
| `Initialize`          | Subscribe to events, render from state loaded in `PreInitialize` | Call a command that raises an event                  |
| `InitializeAsync`     | Await work that needs other services' state                      | Call a command that raises an event                  |
| `PostInitialize`      | Build the world and start the gameplay                           |                                                      |
| `PostInitializeAsync` | Await work that runs once the gameplay has started               |                                                      |

- Anything that raises an event belongs one phase after its handlers subscribe.
- Subscribe in `Initialize` and unsubscribe in `Dispose`.
- Per-frame work goes through `IFrameUpdatable` and the other update interfaces, not the engine's own update callback.

## Disposal

- A handler on an engine or service event ignores events once its owner is disposed.

## Unity

### Installers

- One `RootInstaller` registers the services that live as long as the app. Each scene has an `XSceneInstaller : SceneInstaller`. A large feature gets its own `MonoBehaviour, IInstaller` that the scene installer calls.
- A prefab is registered with a factory lambda: `resolver.Instantiate(_prefab)`.
- A package exposes its registration as `builder.AddX(...)` from an `Integrations/GroveGames.DependencyInjection/` folder.
- Per-frame work does not go in `Update()`.
- A scene's root objects stay inactive until every phase, async ones included, has finished.

### MonoBehaviours

- A MonoBehaviour uses `[Inject] public void Construct(...)` only when it must. Prefer a plain class with constructor injection that drives the MonoBehaviour.
- A pooled component gets its dependencies through an explicit `Initialize(...)` call, not `[Inject]`.

### Disposal

- The container disposes its services on `Application.quitting`, before `OnDisable` and `OnDestroy` run. Game code must not use a service in `OnDestroy` or `OnDisable` during quit.
- A scene's container is disposed when its first `SceneInstaller` is destroyed, before the scene's other `OnDestroy` calls, so they must not use scene services either.
- iOS and Android usually end the app without `Application.quitting`, so `Dispose` does not run there. Save state when the app is paused or loses focus, not in `Dispose`.
