namespace GroveGames.DependencyInjection.Resolution;

internal sealed class TransientBinding : Binding
{
    private readonly InstanceActivator _activator;
    private readonly Container _owner;

    public override InstanceActivator? Activator => _activator;

    public TransientBinding(InstanceActivator activator, Container owner)
    {
        _activator = activator;
        _owner = owner;
    }

    public override object Resolve()
    {
        var instance = _activator.Create();
        _owner.OwnTransient(instance);
        return instance;
    }
}
