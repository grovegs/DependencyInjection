using DependencyInjection.Resolution;

namespace DependencyInjection.Core;

public interface IContainer : IRegistrationResolver, IDisposable
{
    string Name { get; }
    IContainer? Parent { get; }
    IContainer? Create(string name, IInstaller installer);
    void AddChild(IContainer child);
    void RemoveChild(IContainer child);
}
