using Godot;

using GroveGames.DependencyInjection;
using GroveGames.DependencyInjection.Godot;

[GlobalClass]
public partial class GameRootInstaller : RootInstaller
{
    public override void Install(IContainerBuilder builder)
    {
        builder.AddSingleton<ISingleton, Singleton>();
        builder.AddSingleton<RootEntryPoint>();
        GD.Print("Root installed.");
    }
}
