using System.Collections;

namespace DependencyInjection.Core;

internal sealed class DisposableCollection : IDisposableCollection
{
    private readonly Stack<IDisposable> _disposables;

    public DisposableCollection()
    {
        _disposables = new Stack<IDisposable>();
    }

    public void TryAdd(object disposableObject)
    {
        if (disposableObject is IDisposable disposable)
        {
            _disposables.Push(disposable);
        }
    }

    public void Dispose()
    {
        while (_disposables.TryPop(out var disposable))
        {
            disposable.Dispose();
        }
    }

    public IEnumerator<IDisposable> GetEnumerator()
    {
        return _disposables.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
