#if TOOLS
using Godot;

namespace GroveGames.DependencyInjection;

[Tool]
public partial class Plugin : EditorPlugin
{
    public override void _EnterTree()
    {
        var settings = new Settings(new GodotProjectSettings());
        settings.CreateRootInstallerSetting();
        AddAutoloadSingleton(nameof(DependencyInjector), $"res://addons/GroveGames.DependencyInjection/{nameof(DependencyInjector)}.cs");
    }

    public override void _ExitTree()
    {
        RemoveAutoloadSingleton(nameof(DependencyInjector));
    }
}
#endif
