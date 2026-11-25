namespace GroveGames.DependencyInjection;

public interface IContainer : IObjectResolver, IDisposable, IPreFrameUpdatable, IFrameUpdatable, IPostFrameUpdatable, IPhysicsUpdatable
{
    public IContainer? Parent { get; }
    public bool IsDisposed { get; }
    public ValueTask InitializeAsync(CancellationToken cancellationToken = default);
    public IContainer CreateChild(Action<IContainerBuilder> configure);
}
