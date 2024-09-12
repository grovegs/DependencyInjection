using DependencyInjection.Resolution;

namespace DependencyInjection;

public interface IContainer : IRegistrationResolver
{
    string Name { get; }
    IContainer? Parent { get; }
    void AddChild(IContainer child);
    internal void RemoveChild(IContainer child);
    internal void Dispose();
}
