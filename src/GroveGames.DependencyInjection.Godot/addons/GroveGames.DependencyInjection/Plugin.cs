#if TOOLS
using Godot;

namespace GroveGames.DependencyInjection;

[Tool]
public partial class Plugin : EditorPlugin
{
    public override void _EnterTree()
    {
        var settings = GodotProjectSettingsBuilder.Build();

        if (settings.GetSetting<bool>(GodotProjectSettingsKey.AutoLoad))
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