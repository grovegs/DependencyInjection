namespace GroveGames.DependencyInjection;

public interface IInstaller
{
    void Install(IContainerBuilder builder);
}