namespace GroveGames.DependencyInjection;

public interface IInstaller
{
    void Install(IContainerConfigurer containerConfigurer);
}