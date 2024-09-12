namespace DependencyInjection.Core;

internal interface IDisposableCollection : IDisposable, IEnumerable<IDisposable>
{
    void TryAdd(object disposableObject);
}
