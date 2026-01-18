using GroveGames.DependencyInjection.Caching;

namespace GroveGames.DependencyInjection.Tests;

public class RootContainerTests
{
    [Fact]
    public void Constructor_ShouldInitializeRootContainer()
    {
        var mockContainer = new TestContainer
        {
            Name = "TestContainer"
        };

        var rootContainer = new RootContainer(mockContainer);

        Assert.NotNull(rootContainer);
        Assert.Equal(mockContainer.Name, rootContainer.Name);
        Assert.Equal(mockContainer.Parent, rootContainer.Parent);
        Assert.Equal(mockContainer.Cache, rootContainer.Cache);
    }

    [Fact]
    public void AddChild_ShouldDelegateToInternalContainer()
    {
        var mockContainer = new TestContainer();
        var mockChild = new TestContainer();
        var rootContainer = new RootContainer(mockContainer);

        rootContainer.AddChild(mockChild);

        Assert.Single(mockContainer.AddedChildren);
        Assert.Equal(mockChild, mockContainer.AddedChildren[0]);
    }

    [Fact]
    public void RemoveChild_ShouldDelegateToInternalContainer()
    {
        var mockContainer = new TestContainer();
        var mockChild = new TestContainer();
        var rootContainer = new RootContainer(mockContainer);

        rootContainer.RemoveChild(mockChild);

        Assert.Single(mockContainer.RemovedChildren);
        Assert.Equal(mockChild, mockContainer.RemovedChildren[0]);
    }

    [Fact]
    public void Resolve_ShouldDelegateToInternalContainer()
    {
        var mockContainer = new TestContainer();
        var mockObject = new object();
        mockContainer.SetupResolve(typeof(object), mockObject);
        var rootContainer = new RootContainer(mockContainer);

        var result = rootContainer.Resolve(typeof(object));

        Assert.Equal(mockObject, result);
    }

    [Fact]
    public void Dispose_ShouldDelegateToInternalContainer()
    {
        var mockContainer = new TestContainer();
        var rootContainer = new RootContainer(mockContainer);

        rootContainer.Dispose();

        Assert.Equal(1, mockContainer.DisposeCallCount);
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
}
