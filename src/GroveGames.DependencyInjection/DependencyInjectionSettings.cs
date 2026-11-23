namespace GroveGames.DependencyInjection;

public sealed class DependencyInjectionSettings : IDependencyInjectionSettings
{
    private readonly IInstaller? _rootInstaller;

    public IInstaller? RootInstaller => _rootInstaller;

    public DependencyInjectionSettings(IInstaller? rootInstaller)
    {
        _rootInstaller = rootInstaller;
    }
}
