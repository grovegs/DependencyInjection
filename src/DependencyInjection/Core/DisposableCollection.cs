using System.Collections.Generic;
using System;

namespace DependencyInjection.Core
{
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
    }
}
