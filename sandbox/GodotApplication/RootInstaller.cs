using GroveGames.DependencyInjection;
using Godot;

public partial class RootInstaller : RootInstallerBase
{
	public override void Install(IContainerConfigurer containerConfigurer)
	{
		GD.Print("Test");
	}
}
