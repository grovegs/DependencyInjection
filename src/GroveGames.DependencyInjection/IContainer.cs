namespace GroveGames.DependencyInjection;

public interface IContainer : IObjectResolver, IDisposable
{
    public event Action<IContainer>? DescendantInitialized;
    public event Action<IContainer>? DescendantDisposed;
    public IContainer? Parent { get; }
    public bool IsDisposed { get; }
    public ValueTask InitializeAsync(CancellationToken cancellationToken = default);
    public IContainer CreateChild(Action<IContainerBuilder> configure);
}
