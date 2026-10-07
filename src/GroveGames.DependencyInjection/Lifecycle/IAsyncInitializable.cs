namespace GroveGames.DependencyInjection;

public interface IAsyncInitializable
{
    public ValueTask InitializeAsync(CancellationToken cancellationToken);
}
