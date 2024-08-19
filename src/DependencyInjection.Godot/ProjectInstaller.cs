using DependencyInjection.Core;
using Godot;

namespace DependencyInjection.Godot;

public sealed class ProjectInstaller : Node, IInstaller
{
    public void Install(IContainerConfigurer containerConfigurer)
    {
        containerConfigurer.AddSingleton<IA, A>();
    }
}
