namespace DependencyInjection;

public interface IInstaller
{
    void Install(IContainerConfigurer containerConfigurer);
}
