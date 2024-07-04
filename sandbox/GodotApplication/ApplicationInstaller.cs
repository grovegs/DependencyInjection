using Godot;
using DependencyInjection.Core;

public partial class ApplicationInstaller : Node, IInstaller
{
	public void Install(IContainerConfigurer containerConfigurer)
	{
		GD.Print("Test");
	}
}
