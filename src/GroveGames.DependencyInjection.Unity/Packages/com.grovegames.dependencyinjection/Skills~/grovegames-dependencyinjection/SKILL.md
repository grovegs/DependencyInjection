---
name: grovegames-dependencyinjection
description: Rules for wiring game code with GroveGames.DependencyInjection, covering installers, registration order, the initialize phases, injecting MonoBehaviours and disposal. Use when adding or changing an installer, registering a service, implementing IInitializable or an update interface, or debugging startup or shutdown order.
---

# Using GroveGames.DependencyInjection

Rules for game code that is wired with this package. They hold in every game. The API is in the package's `README.md`.

## Installers

- One `RootInstaller` registers the services that live as long as the app. Each scene has an `XSceneInstaller : SceneInstaller`. A large feature gets its own `MonoBehaviour, IInstaller` that the scene installer calls.
- Register with `AddSingleton<IX, X>()`, `AddSingleton(instance)`, or a factory lambda for prefabs (`resolver.Instantiate(_prefab)`) and for arguments the container cannot inject.
- Registration order is execution order.
- A package exposes its registration as `builder.AddX(...)` from an `Integrations/GroveGames.DependencyInjection/` folder.

## Lifecycle

The container builds every object first, then runs three phases. Each phase finishes before the next starts.

| Phase | Do | Do not |
| --- | --- | --- |
| Constructor | Store dependencies, create collections and delegates | Call a dependency. It exists but has not initialized |
| `PreInitialize` | Load and validate saved state and definitions | Raise events or read another service's state |
| `Initialize` | Subscribe to events, render from state loaded in `PreInitialize` | Call a command that raises an event |
| `PostInitialize` | Build the world and start the gameplay | |

- Anything that raises an event belongs one phase after its handlers subscribe.
- Subscribe in `Initialize` and unsubscribe in `Dispose`.
- Per-frame work goes through `IFrameUpdatable` and the other update interfaces, not `Update()`.

## MonoBehaviours

- A MonoBehaviour uses `[Inject] public void Construct(...)` only when it must. Prefer a plain class with constructor injection that drives the MonoBehaviour.
- A pooled component gets its dependencies through an explicit `Initialize(...)` call, not `[Inject]`.

## Disposal

- The container disposes its services on `Application.quitting`, before `OnDisable` and `OnDestroy` run. Game code must not use a service in `OnDestroy` or `OnDisable` during quit.
- A handler on an engine or service event ignores events once its owner is disposed.
