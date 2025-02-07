using GroveGames.DependencyInjection.Caching;

namespace GroveGames.DependencyInjection;

public sealed class NullContainer : IContainer
{
    public string Name => string.Empty;
    public IContainer Parent => null!;
    public IContainerCache Cache => null!;

    public void Dispose()
    {
    }

    public void AddChild(IContainer child)
    {
    }

    public void RemoveChild(IContainer child)
    {
    }

    public object Resolve(Type registrationType)
    {
        return null!;
    }
}