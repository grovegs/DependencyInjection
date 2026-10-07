namespace GroveGames.DependencyInjection;

public interface IAsyncPreInitializable
{
    public ValueTask PreInitializeAsync(CancellationToken cancellationToken);
}
