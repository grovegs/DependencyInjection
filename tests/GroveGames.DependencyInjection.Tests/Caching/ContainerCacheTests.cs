using GroveGames.DependencyInjection.Caching;

namespace GroveGames.DependencyInjection.Tests.Caching;

public class ContainerCacheTests
{
    [Fact]
    public void Shared_ShouldReturnSingletonInstance()
    {
        var instance1 = ContainerCache.Shared;
        var instance2 = ContainerCache.Shared;

        Assert.Same(instance1, instance2);
    }

    [Fact]
    public void Find_ShouldReturnContainer_WhenPathIsValid()
    {
        var rootContainer = new TestContainer
        {
            Name = string.Empty,
            Parent = null!
        };
        var childContainer = new TestContainer
        {
            Name = "child",
            Parent = rootContainer
        };
        var grandchildContainer = new TestContainer
        {
            Name = "grandchild",
            Parent = childContainer
        };
        var containerCache = new ContainerCache();
        containerCache.Add(rootContainer);
        containerCache.Add(childContainer);
        containerCache.Add(grandchildContainer);

        var result = containerCache.Find("/child/grandchild");

        Assert.Equal(grandchildContainer, result);
    }

    [Fact]
    public void Find_ShouldReturnEmptyNameContainer_WhenPathIsEmpty()
    {
        var rootContainer = new TestContainer
        {
            Name = string.Empty,
            Parent = null!
        };
        var containerCache = new ContainerCache();
        containerCache.Add(rootContainer);

        var result = containerCache.Find(string.Empty);

        Assert.Equal(rootContainer, result);
    }

    [Fact]
    public void Find_ShouldReturnEmptyNameContainer_WhenPathIsSlash()
    {
        var rootContainer = new TestContainer
        {
            Name = string.Empty,
            Parent = null!
        };
        var containerCache = new ContainerCache();
        containerCache.Add(rootContainer);

        var result = containerCache.Find("/");

        Assert.Equal(rootContainer, result);
    }

    [Fact]
    public void Find_ShouldReturnNull_WhenPathDoesNotMatch()
    {
        var rootContainer = new TestContainer
        {
            Name = "root",
            Parent = null!
        };
        var containerCache = new ContainerCache();
        containerCache.Add(rootContainer);

        var result = containerCache.Find("nonexistent/path");

        Assert.Null(result);
    }

    [Fact]
    public void Add_ShouldAddContainerToCache()
    {
        var container = new TestContainer
        {
            Name = "test"
        };
        var containerCache = new ContainerCache();

        containerCache.Add(container);

        var result = containerCache.Find("test");
        Assert.Equal(container, result);
    }

    [Fact]
    public void Remove_ShouldRemoveContainerFromCache()
    {
        var container = new TestContainer
        {
            Name = "test"
        };
        var containerCache = new ContainerCache();
        containerCache.Add(container);

        containerCache.Remove(container);

        var result = containerCache.Find("test");
        Assert.Null(result);
    }

    [Fact]
    public void Clear_ShouldRemoveAllContainersFromCache()
    {
        var container1 = new TestContainer
        {
            Name = "test1"
        };
        var container2 = new TestContainer
        {
            Name = "test2"
        };
        var containerCache = new ContainerCache();
        containerCache.Add(container1);
        containerCache.Add(container2);

        containerCache.Clear();

        var result1 = containerCache.Find("test1");
        var result2 = containerCache.Find("test2");
        Assert.Null(result1);
        Assert.Null(result2);
    }

    private sealed class TestContainer : IContainer
    {
        public string Name { get; set; } = string.Empty;
        public IContainer Parent { get; set; } = null!;
        public IContainerCache Cache { get; set; } = null!;

        public void AddChild(IContainer child)
        {
        }

        public void RemoveChild(IContainer child)
        {
        }

        public object Resolve(Type registrationType)
        {
            return null!;
        }

        public void Dispose()
        {
        }
    }
}
