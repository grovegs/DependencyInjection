using GroveGames.DependencyInjection.Resolution;

namespace GroveGames.DependencyInjection;

public interface IContainer : IRegistrationResolver, IDisposable
{
    string Name { get; }
    IContainer? Parent { get; }
    void AddChild(IContainer child);
    void RemoveChild(IContainer child);
}