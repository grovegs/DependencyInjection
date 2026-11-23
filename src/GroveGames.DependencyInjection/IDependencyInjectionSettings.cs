namespace GroveGames.DependencyInjection;

public interface IDependencyInjectionSettings
{
    public IInstaller? RootInstaller { get; }
}
