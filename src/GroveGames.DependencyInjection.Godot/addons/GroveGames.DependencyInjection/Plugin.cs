#if TOOLS
using Godot;

namespace GroveGames.DependencyInjection;

[Tool]
public partial class Plugin : EditorPlugin
{
    public override void _EnterTree()
    {
        GodotSettings.CreateIfNotExist();

        if (GodotSettings.AutoLoad.Value)
        {
            AddAutoloadSingleton(nameof(RootContainerBootstrapper), $"res://addons/GroveGames.DependencyInjection/{nameof(RootContainerBootstrapper)}.cs");
        }
    }

    public override void _ExitTree()
    {
        RemoveAutoloadSingleton(nameof(RootContainerBootstrapper));
    }
}
#endif