using Godot;

namespace GroveGames.DependencyInjection;

public abstract partial class RootInstaller : Resource, IInstaller
{
    public abstract void Install(IContainerBuilder builder);
}
