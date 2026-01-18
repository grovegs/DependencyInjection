using System.Collections;
using GroveGames.DependencyInjection.Collections;
using GroveGames.DependencyInjection.Resolution;

namespace GroveGames.DependencyInjection.Tests.Resolution;

public class InitializedObjectResolverTests
{
    [Fact]
    public void Resolve_ShouldReturnImplementationInstance()
    {
        var mockImplementation = new TestClassWithInjectMethod();
        var mockRegistrationResolver = new TestObjectResolver();
        var mockDisposableCollection = new TestDisposableCollection();
        var objectResolver = new InitializedObjectResolver(
            mockImplementation,
            mockRegistrationResolver,
            mockDisposableCollection
        );

        var result = objectResolver.Resolve();

        Assert.NotNull(result);
        Assert.Equal(mockImplementation, result);
    }

    [Fact]
    public void Resolve_ShouldInjectDependencies()
    {
        var mockImplementation = new TestClassWithInjectMethod();
        var mockRegistrationResolver = new TestObjectResolver();
        mockRegistrationResolver.SetupResolve(_ => "InjectedString");
        var mockDisposableCollection = new TestDisposableCollection();
        var objectResolver = new InitializedObjectResolver(
            mockImplementation,
            mockRegistrationResolver,
            mockDisposableCollection
        );

        objectResolver.Resolve();

        Assert.True(mockImplementation.Initialized);
        Assert.Equal("InjectedString", mockImplementation.StringValue);
    }

    [Fact]
    public void Resolve_ShouldAddToDisposableCollection()
    {
        var mockImplementation = new TestClassWithInjectMethod();
        var mockRegistrationResolver = new TestObjectResolver();
        var mockDisposableCollection = new TestDisposableCollection();
        var objectResolver = new InitializedObjectResolver(
            mockImplementation,
            mockRegistrationResolver,
            mockDisposableCollection
        );

        objectResolver.Resolve();

        Assert.Single(mockDisposableCollection.AddedObjects);
        Assert.Equal(mockImplementation, mockDisposableCollection.AddedObjects[0]);
    }

    private sealed class TestClassWithInjectMethod : IDisposable
    {
        public bool Initialized { get; private set; } = false;
        public bool Disposed { get; private set; }
        public string? StringValue { get; private set; }

        public void Dispose()
        {
            Disposed = true;
        }

        [Inject]
        public void Initialize(string value)
        {
            Initialized = true;
            StringValue = value;
        }
    }

    private sealed class TestObjectResolver : IObjectResolver
    {
        private readonly Dictionary<Type, object> _returnValues = new();
        private Func<Type, object>? _resolveFunc;

        public void SetupResolve(Type type, object returnValue)
        {
            _returnValues[type] = returnValue;
        }

        public void SetupResolve(Func<Type, object> resolveFunc)
        {
            _resolveFunc = resolveFunc;
        }

        public object Resolve(Type registrationType)
        {
            if (_resolveFunc != null)
            {
                return _resolveFunc(registrationType);
            }

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
