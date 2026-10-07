namespace GroveGames.DependencyInjection;

public interface IContainer : IObjectResolver, IDisposable
{
    public IContainer? Parent { get; }
    public bool IsInitialized { get; }
    public bool IsDisposed { get; }
    public ValueTask InitializeAsync(CancellationToken cancellationToken = default);
    public IContainer CreateChild(Action<IContainerBuilder> configure);
    public IReadOnlyList<T> ResolveAll<T>() where T : class;
    public void AddDisposable(IDisposable disposable);
}
