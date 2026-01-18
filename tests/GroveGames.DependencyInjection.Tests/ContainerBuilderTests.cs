using System.Collections;
using GroveGames.DependencyInjection.Caching;
using GroveGames.DependencyInjection.Collections;
using GroveGames.DependencyInjection.Resolution;

namespace GroveGames.DependencyInjection.Tests;

public class ContainerBuilderTests
{
    [Fact]
    public void AddSingleton_WithInstance_ShouldAddSingletonResolver()
    {
        var registrationType = typeof(object);
        var implementationInstance = new object();
        var mockResolver = new TestContainerResolver();
        var mockParent = new TestContainer();
        var mockCache = new TestContainerCache();
        var containerBuilder = new ContainerBuilder("TestContainer", mockParent, mockResolver, mockCache);

        containerBuilder.AddSingleton(registrationType, implementationInstance);

        Assert.Single(mockResolver.AddedResolvers);
        Assert.Equal(registrationType, mockResolver.AddedResolvers[0].Type);
        Assert.IsType<SingletonResolver>(mockResolver.AddedResolvers[0].Resolver);
    }

    [Fact]
    public void AddSingleton_WithFactory_ShouldAddSingletonResolver()
    {
        var registrationType = typeof(object);
        var mockResolver = new TestContainerResolver();
        var mockParent = new TestContainer();
        var mockCache = new TestContainerCache();
        var factory = new Func<object>(() => new object());
        var containerBuilder = new ContainerBuilder("TestContainer", mockParent, mockResolver, mockCache);

        containerBuilder.AddSingleton(registrationType, factory);

        Assert.Single(mockResolver.AddedResolvers);
        Assert.Equal(registrationType, mockResolver.AddedResolvers[0].Type);
        Assert.IsType<SingletonResolver>(mockResolver.AddedResolvers[0].Resolver);
    }

    [Fact]
    public void AddSingleton_WithType_ShouldAddSingletonResolver()
    {
        var registrationType = typeof(object);
        var implementationType = typeof(object);
        var mockResolver = new TestContainerResolver();
        var mockParent = new TestContainer();
        var mockCache = new TestContainerCache();
        var containerBuilder = new ContainerBuilder("TestContainer", mockParent, mockResolver, mockCache);

        containerBuilder.AddSingleton(registrationType, implementationType);

        Assert.Single(mockResolver.AddedResolvers);
        Assert.Equal(registrationType, mockResolver.AddedResolvers[0].Type);
        Assert.IsType<SingletonResolver>(mockResolver.AddedResolvers[0].Resolver);
    }

    [Fact]
    public void AddTransient_WithType_ShouldAddTransientResolver()
    {
        var registrationType = typeof(object);
        var implementationType = typeof(object);
        var mockResolver = new TestContainerResolver();
        var mockParent = new TestContainer();
        var mockCache = new TestContainerCache();
        var containerBuilder = new ContainerBuilder("TestContainer", mockParent, mockResolver, mockCache);

        containerBuilder.AddTransient(registrationType, implementationType);

        Assert.Single(mockResolver.AddedResolvers);
        Assert.Equal(registrationType, mockResolver.AddedResolvers[0].Type);
        Assert.IsType<TransientResolver>(mockResolver.AddedResolvers[0].Resolver);
    }

    [Fact]
    public void AddTransient_WithFactory_ShouldAddTransientResolver()
    {
        var registrationType = typeof(object);
        var mockResolver = new TestContainerResolver();
        var mockParent = new TestContainer();
        var mockCache = new TestContainerCache();
        var factory = new Func<object>(() => new object());
        var containerBuilder = new ContainerBuilder("TestContainer", mockParent, mockResolver, mockCache);

        containerBuilder.AddTransient(registrationType, factory);

        Assert.Single(mockResolver.AddedResolvers);
        Assert.Equal(registrationType, mockResolver.AddedResolvers[0].Type);
        Assert.IsType<TransientResolver>(mockResolver.AddedResolvers[0].Resolver);
    }

    [Fact]
    public void AddTransient_WithFactory_ShouldCreateNewInstanceEachTime()
    {
        var registrationType = typeof(object);
        var instanceCount = 0;
        var mockResolver = new TestContainerResolver();
        var mockParent = new TestContainer();
        var mockCache = new TestContainerCache();
        var factory = new Func<object>(() =>
        {
            instanceCount++;
            return new object();
        });
        var containerBuilder = new ContainerBuilder("TestContainer", mockParent, mockResolver, mockCache);

        containerBuilder.AddTransient(registrationType, factory);

        factory();
        factory();

        Assert.Equal(2, instanceCount);
        Assert.Single(mockResolver.AddedResolvers);
        Assert.Equal(registrationType, mockResolver.AddedResolvers[0].Type);
        Assert.IsType<TransientResolver>(mockResolver.AddedResolvers[0].Resolver);
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
}
