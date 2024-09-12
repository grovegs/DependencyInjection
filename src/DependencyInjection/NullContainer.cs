namespace DependencyInjection;

public sealed class NullContainer : IContainer
{
    public string Name => string.Empty;

    public IContainer? Parent => null;

    public void AddChild(IContainer child)
    {
    }

    public void Dispose()
    {
    }

    public void RemoveChild(IContainer child)
    {
    }

    public object Resolve(Type registrationType)
    {
        throw new InvalidOperationException("Cannot resolve from a null container.");
    }
}
