using GroveGames.DependencyInjection.Collections;

namespace GroveGames.DependencyInjection.Tests.Collections;

public class DisposableCollectionTests
{
    [Fact]
    public void TryAdd_AddsDisposableObjectToCollection()
    {
        var disposable = new TestDisposable();
        var collection = new DisposableCollection();

        collection.TryAdd(disposable);

        Assert.NotEmpty(collection);
    }

    [Fact]
    public void TryAdd_DoesNotAddNonDisposableObjectToCollection()
    {
        var nonDisposableObject = new object();
        var collection = new DisposableCollection();

        collection.TryAdd(nonDisposableObject);

        Assert.Empty(collection);
    }

    [Fact]
    public void Dispose_DisposesAllDisposablesInCollection()
    {
        var disposable1 = new TestDisposable();
        var disposable2 = new TestDisposable();
        var collection = new DisposableCollection();
        collection.TryAdd(disposable1);
        collection.TryAdd(disposable2);

        collection.Dispose();

        Assert.Equal(1, disposable1.DisposeCallCount);
        Assert.Equal(1, disposable2.DisposeCallCount);
    }

    [Fact]
    public void Dispose_CanBeCalledMultipleTimes()
    {
        var disposable = new TestDisposable();
        var collection = new DisposableCollection();
        collection.TryAdd(disposable);

        collection.Dispose();
        collection.Dispose();

        Assert.Equal(1, disposable.DisposeCallCount);
    }

    private sealed class TestDisposable : IDisposable
    {
        public int DisposeCallCount { get; private set; }

        public void Dispose()
        {
            DisposeCallCount++;
        }
    }
}
