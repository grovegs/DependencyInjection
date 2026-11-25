namespace GroveGames.DependencyInjection.Resolution;

internal sealed class SingletonBinding : Binding
{
    private readonly InstanceActivator _activator;
    private readonly Container _owner;
    private object? _instance;

    public override InstanceActivator? Activator => _activator;

    public SingletonBinding(InstanceActivator activator, Container owner)
    {
        _activator = activator;
        _owner = owner;
    }

    public override object Resolve(Container requester)
    {
        return _instance ?? Create();
    }

    private object Create()
    {
        var instance = _activator.Create(_owner);
        _instance = instance;
        _owner.OwnSingleton(instance);
        return instance;
    }
}
