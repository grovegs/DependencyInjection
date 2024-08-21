#if TOOLS
using Godot;

namespace DependencyInjection.Godot;

[Tool]
public partial class DependencyInjectionPlugin : EditorPlugin
{
	public override void _EnterTree()
	{
		AddAutoloadSingleton(nameof(ApplicationInitializer), $"res://addons/DependencyInjection/{nameof(ApplicationInitializer)}.cs");
	}

	public override void _ExitTree()
	{
		RemoveAutoloadSingleton(nameof(ApplicationInitializer));
	}
}
#endif
