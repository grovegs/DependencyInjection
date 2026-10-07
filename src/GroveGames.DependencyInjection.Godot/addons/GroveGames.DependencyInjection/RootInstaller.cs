using Godot;

namespace GroveGames.DependencyInjection.Godot;

public abstract partial class RootInstaller : Resource, IInstaller
{
    public abstract void Install(IContainerBuilder builder);
}
