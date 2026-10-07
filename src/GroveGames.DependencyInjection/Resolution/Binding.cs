namespace GroveGames.DependencyInjection.Resolution;

internal abstract class Binding
{
    public virtual InstanceActivator? Activator => null;

    public abstract object Resolve();
}
