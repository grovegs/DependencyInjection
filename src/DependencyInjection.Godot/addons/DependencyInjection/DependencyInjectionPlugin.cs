#if TOOLS
using Godot;

namespace DependencyInjection;

[Tool]
public partial class DependencyInjectionPlugin : EditorPlugin
{
    public override void _EnterTree()
    {
        Settings.CreateRootInstallerSetting();
        AddAutoloadSingleton(nameof(DependencyInjector), $"res://addons/DependencyInjection/{nameof(DependencyInjector)}.cs");
    }

    public override void _ExitTree()
    {
        RemoveAutoloadSingleton(nameof(DependencyInjector));
    }
}
#endif
