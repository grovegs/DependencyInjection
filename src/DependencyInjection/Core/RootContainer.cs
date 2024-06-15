namespace DependencyInjection.Core;

internal sealed class RootContainer : IContainer
{
    public RootContainer()
    {
    }

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
        return null;
    }
}
