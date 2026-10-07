namespace GroveGames.DependencyInjection.Resolution;

internal abstract class InstanceActivator
{
    public abstract Type ImplementationType { get; }
    public virtual ReadOnlySpan<Binding> Dependencies => ReadOnlySpan<Binding>.Empty;

    public abstract object Create();
}
