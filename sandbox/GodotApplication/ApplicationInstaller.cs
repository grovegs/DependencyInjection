using DependencyInjection;
using Godot;

public partial class ApplicationInstaller : RootInstaller
{
	public override void Install(IContainerConfigurer containerConfigurer)
	{
		GD.Print("Test");
	}
}
