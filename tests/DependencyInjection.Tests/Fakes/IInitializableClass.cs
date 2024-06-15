using DependencyInjection.Core;

namespace DependencyInjection.Tests.Fakes;

internal interface IInitializableClass : IInitializable
{
    public bool Initialized { get; }
}
