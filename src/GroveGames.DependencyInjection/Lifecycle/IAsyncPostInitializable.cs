namespace GroveGames.DependencyInjection;

public interface IAsyncPostInitializable
{
    public ValueTask PostInitializeAsync(CancellationToken cancellationToken);
}
