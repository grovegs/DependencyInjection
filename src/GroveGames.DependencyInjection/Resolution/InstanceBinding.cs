namespace GroveGames.DependencyInjection.Resolution;

internal sealed class InstanceBinding : Binding
{
    private readonly object _instance;

    public InstanceBinding(object instance)
    {
        _instance = instance;
    }

    public override object Resolve(Container requester)
    {
        return _instance;
    }
}
