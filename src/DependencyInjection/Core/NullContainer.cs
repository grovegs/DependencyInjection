namespace DependencyInjection.Core;

public sealed class NullContainer : IContainer
{
    public string Name => string.Empty;

    public IContainer? Parent => null;

    public void AddChild(IContainer child)
    {
    }

    public IContainer? Create(string name, IInstaller installer)
    {
        return null;
    }

    public void Dispose()
    {
    }

    public void RemoveChild(IContainer child)
    {
    }

    public object? Resolve(Type registrationType)
    {
        return null;
    }
}
