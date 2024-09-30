using DependencyInjection;
using Godot;

public partial class ApplicationInstaller : Installer
{
	public override void Install(IContainerConfigurer containerConfigurer)
	{
		GD.Print("Test");
	}
}
