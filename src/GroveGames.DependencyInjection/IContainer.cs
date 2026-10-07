namespace GroveGames.DependencyInjection;

public interface IContainer : IObjectResolver, IDisposable
{
    public IContainer? Parent { get; }
    public bool IsInitialized { get; }
    public bool IsDisposed { get; }
    public ValueTask InitializeAsync(CancellationToken cancellationToken = default);
    public IContainer CreateChild(Action<IContainerBuilder> configure);
    public void Update(float deltaTime);
    public void FixedUpdate(float deltaTime);
    public void LateUpdate(float deltaTime);
}
