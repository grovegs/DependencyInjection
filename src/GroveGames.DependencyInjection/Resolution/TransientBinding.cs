namespace GroveGames.DependencyInjection.Resolution;

internal sealed class TransientBinding : Binding
{
    private readonly InstanceActivator _activator;

    public override InstanceActivator? Activator => _activator;

    public TransientBinding(InstanceActivator activator)
    {
        _activator = activator;
    }

    public override object Resolve(Container requester)
    {
        var instance = _activator.Create(requester);
        requester.OwnTransient(instance);
        return instance;
    }
}
