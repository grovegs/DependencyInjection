using System.Collections;
using System.Reflection;
using GroveGames.DependencyInjection.Caching;
using GroveGames.DependencyInjection.Collections;
using GroveGames.DependencyInjection.Resolution;

namespace GroveGames.DependencyInjection.Tests;

public class ContainerTests
{
    private Container CreateContainer(
        string name = "TestContainer",
        TestContainerResolver? resolverMock = null,
        TestContainerCache? cacheMock = null,
        TestDisposableCollection? disposablesMock = null,
        TestContainer? parentMock = null
    )
    {
        resolverMock ??= new TestContainerResolver();
        cacheMock ??= new TestContainerCache();
        disposablesMock ??= new TestDisposableCollection();
        parentMock ??= new TestContainer();

        return new Container(
            name,
            parentMock,
            resolverMock,
            cacheMock,
            disposablesMock
        );
    }

    [Fact]
    public void Constructor_ShouldInitializeContainer()
    {
        var mockParentContainer = new TestContainer();
        var mockCache = new TestContainerCache();

        var container = CreateContainer(parentMock: mockParentContainer, cacheMock: mockCache);

        Assert.Equal("TestContainer", container.Name);
        Assert.Equal(mockParentContainer, container.Parent);
        Assert.Single(mockParentContainer.AddedChildren);
        Assert.Equal(container, mockParentContainer.AddedChildren[0]);
        Assert.Single(mockCache.AddedContainers);
        Assert.Equal(container, mockCache.AddedContainers[0]);
    }

    [Fact]
    public void AddChild_ShouldAddChildContainer()
    {
        var container = CreateContainer();
        var childMock = new TestContainer { Name = "ChildContainer" };

        container.AddChild(childMock);

        var childrenField = typeof(Container).GetField("_children", BindingFlags.NonPublic | BindingFlags.Instance);
        var children = childrenField?.GetValue(container) as List<IContainer>;
        Assert.Contains(childMock, children!);
    }

    [Fact]
    public void AddChild_ShouldThrowArgumentException_WhenDuplicateChildIsAdded()
    {
        var container = CreateContainer();
        var childMock = new TestContainer { Name = "ChildContainer" };
        container.AddChild(childMock);

        var ex = Assert.Throws<ArgumentException>(() => container.AddChild(childMock));
        Assert.Contains("A child container with the same name", ex.Message);
    }

    [Fact]
    public void RemoveChild_ShouldRemoveChildContainer()
    {
        var container = CreateContainer();
        var childMock = new TestContainer { Name = "ChildContainer" };
        container.AddChild(childMock);

        container.RemoveChild(childMock);

        var childrenField = typeof(Container).GetField("_children", BindingFlags.NonPublic | BindingFlags.Instance);
        var children = childrenField?.GetValue(container) as List<IContainer>;
        Assert.DoesNotContain(childMock, children!);
    }

    [Fact]
    public void Dispose_ShouldDisposeAllResources()
    {
        var mockDisposables = new TestDisposableCollection();
        var mockCache = new TestContainerCache();
        var mockParentContainer = new TestContainer();

        var container = CreateContainer(disposablesMock: mockDisposables, cacheMock: mockCache, parentMock: mockParentContainer);

        container.Dispose();

        Assert.Equal(1, mockDisposables.DisposeCallCount);
        Assert.Single(mockCache.RemovedContainers);
        Assert.Equal(container, mockCache.RemovedContainers[0]);
        Assert.Single(mockParentContainer.RemovedChildren);
        Assert.Equal(container, mockParentContainer.RemovedChildren[0]);
    }

    [Fact]
    public void Resolve_ShouldUseResolverToResolveType()
    {
        var mockResolver = new TestContainerResolver();
        var container = CreateContainer(resolverMock: mockResolver);

        var mockObject = new object();
        mockResolver.SetupResolve(typeof(object), mockObject);

        var result = container.Resolve(typeof(object));

        Assert.Equal(mockObject, result);
    }

    private sealed class TestContainer : IContainer
    {
        private readonly List<IContainer> _addedChildren = new();
        private readonly List<IContainer> _removedChildren = new();
        private readonly Dictionary<Type, object> _returnValues = new();

        public string Name { get; set; } = string.Empty;
        public IContainer Parent { get; set; } = null!;
        public IContainerCache Cache { get; set; } = null!;

        public int DisposeCallCount { get; private set; }
        public IReadOnlyList<IContainer> AddedChildren => _addedChildren;
        public IReadOnlyList<IContainer> RemovedChildren => _removedChildren;

        public void SetupResolve(Type type, object returnValue)
        {
            _returnValues[type] = returnValue;
        }

        public void AddChild(IContainer child)
        {
            _addedChildren.Add(child);
        }

        public void RemoveChild(IContainer child)
        {
            _removedChildren.Add(child);
        }

        public object Resolve(Type registrationType)
        {
            return _returnValues.TryGetValue(registrationType, out var value) ? value : null!;
        }

        public void Dispose()
        {
            DisposeCallCount++;
        }
    }

    private sealed class TestContainerResolver : IContainerResolver
    {
        private readonly Dictionary<Type, object> _returnValues = new();
        private readonly List<(Type Type, IInstanceResolver Resolver)> _addedResolvers = new();

        public int ClearCallCount { get; private set; }
        public IReadOnlyList<(Type Type, IInstanceResolver Resolver)> AddedResolvers => _addedResolvers;

        public void SetupResolve(Type type, object returnValue)
        {
            _returnValues[type] = returnValue;
        }

        public object Resolve(Type registrationType)
        {
            return _returnValues.TryGetValue(registrationType, out var value) ? value : null!;
        }

        public void AddResolver(Type registrationType, IInstanceResolver resolver)
        {
            _addedResolvers.Add((registrationType, resolver));
        }

        public void Clear()
        {
            ClearCallCount++;
        }
    }

    private sealed class TestContainerCache : IContainerCache
    {
        private readonly List<IContainer> _addedContainers = new();
        private readonly List<IContainer> _removedContainers = new();

        public int ClearCallCount { get; private set; }
        public IReadOnlyList<IContainer> AddedContainers => _addedContainers;
        public IReadOnlyList<IContainer> RemovedContainers => _removedContainers;

        public IContainer? Find(in ReadOnlySpan<char> path)
        {
            return null;
        }

        public void Add(IContainer container)
        {
            _addedContainers.Add(container);
        }

        public void Remove(IContainer container)
        {
            _removedContainers.Add(container);
        }

        public void Clear()
        {
            ClearCallCount++;
        }
    }

    private sealed class TestDisposableCollection : IDisposableCollection
    {
        private readonly List<object> _addedObjects = new();

        public int DisposeCallCount { get; private set; }
        public IReadOnlyList<object> AddedObjects => _addedObjects;

        public void TryAdd(object disposableObject)
        {
            _addedObjects.Add(disposableObject);
        }

        public void Dispose()
        {
            DisposeCallCount++;
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
