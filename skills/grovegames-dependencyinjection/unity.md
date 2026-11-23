## Unity

### Installers

- One `RootInstaller` registers the services that live as long as the app. Each scene has an `XSceneInstaller : SceneInstaller`. A large feature gets its own `MonoBehaviour, IInstaller` that the scene installer calls.
- A prefab is registered with a factory lambda: `resolver.Instantiate(_prefab)`.
- A package exposes its registration as `builder.AddX(...)` from an `Integrations/GroveGames.DependencyInjection/` folder.
- Per-frame work does not go in `Update()`.

### MonoBehaviours

- A MonoBehaviour uses `[Inject] public void Construct(...)` only when it must. Prefer a plain class with constructor injection that drives the MonoBehaviour.
- A pooled component gets its dependencies through an explicit `Initialize(...)` call, not `[Inject]`.

### Disposal

- The container disposes its services on `Application.quitting`, before `OnDisable` and `OnDestroy` run. Game code must not use a service in `OnDestroy` or `OnDisable` during quit.
