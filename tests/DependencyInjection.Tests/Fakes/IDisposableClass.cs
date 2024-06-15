namespace DependencyInjection.Tests.Fakes;

internal interface IDisposableClass : IDisposable
{
    bool Disposed { get; }
}
