namespace DependencyInjection.Core;

internal interface IDisposableCollection : IDisposable
{
    void TryAdd(object disposableObject);
}
