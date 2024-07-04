using Godot;

public partial class ApplicationInitializer : Node
{
	private DependencyInjection.Godot.ApplicationInitializer _applicationInitializer;

	public override void _EnterTree()
	{
		_applicationInitializer = new DependencyInjection.Godot.ApplicationInitializer();
	}

	public override void _ExitTree()
	{
		_applicationInitializer.Dispose();
	}

	public override void _Notification(int what)
	{
		GD.Print(what);
	}
}
