namespace DependencyInjection.Core;

public interface IInstaller
{
    void Install(IContainerConfigurer containerConfigurer);
}
