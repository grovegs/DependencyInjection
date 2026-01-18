using System.Collections;
using GroveGames.DependencyInjection.Collections;
using GroveGames.DependencyInjection.Resolution;

namespace GroveGames.DependencyInjection.Tests.Resolution;

public class UninitializedObjectResolverTests
{
    [Fact]
    public void Resolve_ShouldReturnInstanceOfImplementationType()
    {
        var implementationType = typeof(TestService);
        var mockRegistrationResolver = new TestObjectResolver();
        var mockDisposableCollection = new TestDisposableCollection();
        var objectResolver = new UninitializedObjectResolver(implementationType, mockRegistrationResolver, mockDisposableCollection);

        var result = objectResolver.Resolve();

        Assert.NotNull(result);
        Assert.IsType<TestService>(result);
    }

    [Fact]
    public void Resolve_ShouldAddToDisposableCollection()
    {
        var implementationType = typeof(TestService);
        var mockRegistrationResolver = new TestObjectResolver();
        var mockDisposableCollection = new TestDisposableCollection();
        var objectResolver = new UninitializedObjectResolver(implementationType, mockRegistrationResolver, mockDisposableCollection);

        var result = objectResolver.Resolve();

        Assert.Single(mockDisposableCollection.AddedObjects);
        Assert.Equal(result, mockDisposableCollection.AddedObjects[0]);
    }

    private sealed class TestService
    {
    }

    private sealed class TestObjectResolver : IObjectResolver
    {
        private readonly Dictionary<Type, object> _returnValues = new();

        public void SetupResolve(Type type, object returnValue)
        {
            _returnValues[type] = returnValue;
        }

        public object Resolve(Type registrationType)
        {
            return _returnValues.TryGetValue(registrationType, out var value) ? value : null!;
        }
    }

    private sealed class TestDisposableCollection : IDisposableCollection
    {
        private readonly List<object> _addedObjects = new();

        public IReadOnlyList<object> AddedObjects => _addedObjects;

        public void TryAdd(object disposableObject)
        {
            _addedObjects.Add(disposableObject);
        }

        public void Dispose()
        {
        }

        public IEnumerator<IDisposable> GetEnumerator()
        {
            return Enumerable.Empty<IDisposable>().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
