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
            AddAutoloadSingleton(nameof(GodotRootNode), $"res://addons/GroveGames.DependencyInjection/{nameof(GodotRootNode)}.cs");
        }
    }

    public override void _ExitTree()
    {
        RemoveAutoloadSingleton(nameof(GodotRootNode));
    }
}
#endif