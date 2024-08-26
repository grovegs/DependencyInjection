using DependencyInjection.Core;
using DependencyInjection.Godot;
using Godot;

public sealed partial class MainInstaller : Installer
{
	public override void Install(IContainerConfigurer containerConfigurer)
	{
		GD.Print("Test");
	}
}
