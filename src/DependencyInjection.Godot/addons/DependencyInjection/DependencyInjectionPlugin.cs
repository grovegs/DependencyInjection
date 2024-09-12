#if TOOLS
using Godot;

namespace DependencyInjection;

[Tool]
public partial class DependencyInjectionPlugin : EditorPlugin
{
	public override void _EnterTree()
	{
		AddAutoloadSingleton(nameof(DependencyInjectionRunner), $"res://addons/DependencyInjection/{nameof(DependencyInjectionRunner)}.cs");
	}

	public override void _ExitTree()
	{
		RemoveAutoloadSingleton(nameof(DependencyInjectionRunner));
	}
}
#endif
