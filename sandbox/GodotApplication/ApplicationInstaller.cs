using DependencyInjection.Core;
using DependencyInjection.Godot;
using Godot;

public partial class ApplicationInstaller : NodeInstaller
{
	public override void Install(IContainerConfigurer containerConfigurer)
	{
		GD.Print("Test");
	}
}
