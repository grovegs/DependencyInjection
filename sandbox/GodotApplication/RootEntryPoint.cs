using System.Threading;
using System.Threading.Tasks;

using Godot;

using GroveGames.DependencyInjection;

public sealed class RootEntryPoint : IAsyncInitializable, IInitializable, IProcessable
{
    private readonly ISingleton _singleton;
    private int _frameCount;

    public RootEntryPoint(ISingleton singleton)
    {
        _singleton = singleton;
    }

    public async ValueTask InitializeAsync(CancellationToken cancellationToken)
    {
        await Task.Delay(100, cancellationToken);
        GD.Print("Root initialized asynchronously.");
    }

    public void Initialize()
    {
        GD.Print("Root initialized.");
    }

    public void Process(double delta)
    {
        if (++_frameCount == 10)
        {
            GD.Print("Root updated 10 frames.");
        }
    }
}
