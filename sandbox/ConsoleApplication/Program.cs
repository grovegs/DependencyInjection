using GroveGames.DependencyInjection;

var builder = new ContainerBuilder();
builder.AddSingleton<IClock, SystemClock>();
builder.AddSingleton<Game>();
using var root = builder.Build();
await root.InitializeAsync();

using var scene = root.CreateChild(sceneBuilder => sceneBuilder.AddSingleton<Level>());
await scene.InitializeAsync();

var tickables = new List<ITickable>();
tickables.AddRange(root.ResolveAll<ITickable>());
tickables.AddRange(scene.ResolveAll<ITickable>());

for (var frame = 0; frame < 3; frame++)
{
    for (var i = 0; i < tickables.Count; i++)
    {
        tickables[i].Tick(1f / 60f);
    }
}

public interface ITickable
{
    void Tick(float deltaTime);
}

public interface IClock
{
    DateTime Now { get; }
}

public sealed class SystemClock : IClock
{
    public DateTime Now => DateTime.Now;
}

public sealed class Game : IAsyncInitializable, IInitializable, ITickable, IDisposable
{
    private readonly IClock _clock;

    public Game(IClock clock)
    {
        _clock = clock;
    }

    public async ValueTask InitializeAsync(CancellationToken cancellationToken)
    {
        await Task.Delay(10, cancellationToken);
        Console.WriteLine($"Game loaded at {_clock.Now:T}");
    }

    public void Initialize()
    {
        Console.WriteLine("Game initialized");
    }

    public void Tick(float deltaTime)
    {
        Console.WriteLine($"Game ticked {deltaTime:F4}");
    }

    public void Dispose()
    {
        Console.WriteLine("Game disposed");
    }
}

public sealed class Level : IPostInitializable, ITickable, IDisposable
{
    private readonly Game _game;

    public Level(Game game)
    {
        _game = game;
    }

    public void PostInitialize()
    {
        Console.WriteLine("Level started");
    }

    public void Tick(float deltaTime)
    {
        Console.WriteLine("Level ticked");
    }

    public void Dispose()
    {
        Console.WriteLine("Level disposed");
    }
}
