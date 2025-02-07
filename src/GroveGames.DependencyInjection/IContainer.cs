using GroveGames.DependencyInjection.Caching;
using GroveGames.DependencyInjection.Resolution;

namespace GroveGames.DependencyInjection;

public interface IContainer : IRegistrationResolver, IDisposable
{
    string Name { get; }
    IContainer? Parent { get; }
    IContainerCache? Cache { get; }
    void AddChild(IContainer child);
    void RemoveChild(IContainer child);
}