# GroveGames.DependencyInjection

A lightweight dependency injection framework for .NET and Godot with Native AOT support. Built for game development scenarios where simplicity and performance are critical.

[![Build Status](https://github.com/grovegs/DependencyInjection/actions/workflows/release.yml/badge.svg)](https://github.com/grovegs/DependencyInjection/actions/workflows/release.yml)
[![Tests](https://github.com/grovegs/DependencyInjection/actions/workflows/tests.yml/badge.svg)](https://github.com/grovegs/DependencyInjection/actions/workflows/tests.yml)
[![Latest Release](https://img.shields.io/github/v/release/grovegs/DependencyInjection)](https://github.com/grovegs/DependencyInjection/releases/latest)
[![NuGet](https://img.shields.io/nuget/v/GroveGames.DependencyInjection)](https://www.nuget.org/packages/GroveGames.DependencyInjection)

---

## Features

- **Hierarchical Containers**: Parent-child container relationships with automatic disposal
- **Singleton & Transient Lifetimes**: Register dependencies with appropriate lifetime management
- **Constructor Injection**: Automatic dependency resolution via constructors
- **Method Injection**: Inject dependencies via methods marked with `[Inject]` attribute
- **Factory Registration**: Register dependencies using factory functions
- **Installer Pattern**: Modular dependency registration via `IInstaller` interface
- **Container Caching**: Fast container lookup by path
- **Native AOT Compatible**: Fully supports ahead-of-time compilation for maximum performance

## .NET

Install via NuGet:

```bash
dotnet add package GroveGames.DependencyInjection
```

### Basic Container Setup

```csharp
using GroveGames.DependencyInjection;

var rootContainer = RootContainerFactory.CreateRootContainer(builder =>
{
    builder.AddSingleton<ILogger, ConsoleLogger>();
    builder.AddSingleton<IGameService, GameService>();
    builder.AddTransient<IEnemy, Zombie>();
});

var logger = rootContainer.Resolve<ILogger>();
logger.Log("Hello, World!");

rootContainer.Dispose();
```

### Singleton Registration

Register single instances that persist for the container's lifetime:

```csharp
using GroveGames.DependencyInjection;

var container = RootContainerFactory.CreateRootContainer(builder =>
{
    builder.AddSingleton<IPlayerService, PlayerService>();

    builder.AddSingleton<IAudioManager, AudioManager>();

    var config = new GameConfig { Volume = 0.8f };
    builder.AddSingleton<IGameConfig, GameConfig>(config);

    builder.AddSingleton<IDatabase>(() => new SqliteDatabase("game.db"));
});
```

### Transient Registration

Register types that create new instances on each resolution:

```csharp
using GroveGames.DependencyInjection;

var container = RootContainerFactory.CreateRootContainer(builder =>
{
    builder.AddTransient<IEnemy, Goblin>();

    builder.AddTransient<IBullet>(() => new Bullet(speed: 10f));
});

var enemy1 = container.Resolve<IEnemy>();
var enemy2 = container.Resolve<IEnemy>();
```

### Constructor Injection

Dependencies are automatically resolved via constructor parameters:

```csharp
using GroveGames.DependencyInjection;

public class GameManager
{
    private readonly ILogger _logger;
    private readonly IPlayerService _playerService;

    public GameManager(ILogger logger, IPlayerService playerService)
    {
        _logger = logger;
        _playerService = playerService;
    }
}

var container = RootContainerFactory.CreateRootContainer(builder =>
{
    builder.AddSingleton<ILogger, ConsoleLogger>();
    builder.AddSingleton<IPlayerService, PlayerService>();
    builder.AddSingleton<GameManager>();
});

var gameManager = container.Resolve<GameManager>();
```

### Method Injection

Use the `[Inject]` attribute to inject dependencies after construction:

```csharp
using GroveGames.DependencyInjection;

public class Player
{
    private IInputService _inputService;
    private IAudioService _audioService;

    [Inject]
    public void Initialize(IInputService inputService, IAudioService audioService)
    {
        _inputService = inputService;
        _audioService = audioService;
    }
}

var container = RootContainerFactory.CreateRootContainer(builder =>
{
    builder.AddSingleton<IInputService, InputService>();
    builder.AddSingleton<IAudioService, AudioService>();
    builder.AddSingleton<Player>();
});
```

### Installer Pattern

Use installers for modular dependency registration:

```csharp
using GroveGames.DependencyInjection;

public class CoreInstaller : IInstaller
{
    public void Install(IContainerBuilder builder)
    {
        builder.AddSingleton<ILogger, ConsoleLogger>();
        builder.AddSingleton<IEventBus, EventBus>();
    }
}

public class GameplayInstaller : IInstaller
{
    public void Install(IContainerBuilder builder)
    {
        builder.AddSingleton<IPlayerService, PlayerService>();
        builder.AddSingleton<IEnemyService, EnemyService>();
        builder.AddTransient<IProjectile, Bullet>();
    }
}

var container = RootContainerFactory.CreateRootContainer(builder =>
{
    new CoreInstaller().Install(builder);
    new GameplayInstaller().Install(builder);
});
```

### Hierarchical Containers

Create child containers that inherit from parents:

```csharp
using GroveGames.DependencyInjection;

var rootContainer = RootContainerFactory.CreateRootContainer(builder =>
{
    builder.AddSingleton<ILogger, ConsoleLogger>();
    builder.AddSingleton<IGameConfig, GameConfig>();
});

var levelContainer = ContainerFactory.CreateContainer("Level1", rootContainer, builder =>
{
    builder.AddSingleton<ILevelManager, LevelManager>();
    builder.AddTransient<IEnemy, LevelEnemy>();
});

var logger = levelContainer.Resolve<ILogger>();

var levelManager = levelContainer.Resolve<ILevelManager>();

levelContainer.Dispose();
rootContainer.Dispose();
```

### Container Lookup

Find child containers by path:

```csharp
using GroveGames.DependencyInjection;

var rootContainer = RootContainerFactory.CreateRootContainer(builder => { });

var gameContainer = ContainerFactory.CreateContainer("Game", rootContainer, builder => { });
var uiContainer = ContainerFactory.CreateContainer("UI", gameContainer, builder => { });

var found = rootContainer.FindChild("Root/Game/UI");
```

### Core Components

- **`IContainer`**: Core container interface with Resolve, AddChild, RemoveChild, and Dispose
- **`IRootContainer`**: Root container with FindChild capability for container lookup
- **`IContainerBuilder`**: Builder interface for registering dependencies
- **`IObjectResolver`**: Interface for resolving dependencies by type
- **`IInstaller`**: Interface for modular dependency registration
- **`InjectAttribute`**: Attribute for marking methods for dependency injection
- **`RootContainerFactory`**: Factory for creating root containers
- **`ContainerFactory`**: Factory for creating child containers

## Godot

Install via NuGet:

```bash
dotnet add package GroveGames.DependencyInjection
```

Download the Godot addon from the [latest release](https://github.com/grovegs/DependencyInjection/releases/latest) and extract it to your project's `addons` folder. Enable the addon in Project Settings → Plugins.

```text
res://
├── addons/
│   └── GroveGames.DependencyInjection/
│       ├── plugin.cfg
│       └── ...
└── ...
```

## Architecture

### Performance Optimizations

1. **Hierarchical Resolution**: Child containers fall back to parent for unregistered types
2. **Container Caching**: O(1) container lookup by path using cache
3. **Lazy Singleton Creation**: Singletons created on first resolution
4. **Native AOT**: Full compatibility with ahead-of-time compilation
5. **No Reflection at Runtime**: AOT-friendly design with compile-time analysis

### Container Lifecycle

1. **Creation**: Containers are created via factories with builder configuration
2. **Registration**: Dependencies registered during build phase
3. **Resolution**: Dependencies resolved on demand with automatic injection
4. **Disposal**: Disposing a container disposes all children and registered disposables

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
