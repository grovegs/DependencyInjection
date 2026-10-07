using GroveGames.DependencyInjection;

var builder = new ContainerBuilder();
builder.AddSingleton<IClock, SystemClock>();
builder.AddSingleton<Game>();
using var root = builder.Build();
await root.InitializeAsync();

using var scene = root.CreateChild(sceneBuilder => sceneBuilder.AddSingleton<Level>());
await scene.InitializeAsync();

for (var frame = 0; frame < 3; frame++)
{
    root.Update(1f / 60f);
}

public interface IClock
{
    DateTime Now { get; }
}

public sealed class SystemClock : IClock
{
    public DateTime Now => DateTime.Now;
}

public sealed class Game : IAsyncInitializable, IInitializable, IUpdatable, IDisposable
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

    public void Update(float deltaTime)
    {
        Console.WriteLine($"Game updated {deltaTime:F4}");
    }

    public void Dispose()
    {
        Console.WriteLine("Game disposed");
    }
}

public sealed class Level : IPostInitializable, IUpdatable, IDisposable
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

    public void Update(float deltaTime)
    {
        Console.WriteLine("Level updated");
    }

    public void Dispose()
    {
        Console.WriteLine("Level disposed");
    }
}
