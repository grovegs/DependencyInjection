using DependencyInjection.Core;

namespace DependencyInjection.Tests.Mocks;

internal sealed class MockDisposableCollection : IDisposableCollection
{
    public MockDisposableCollection()
    {
    }

    public void Dispose()
    {
    }

    public void TryAdd(object disposableObject)
    {
    }
}
