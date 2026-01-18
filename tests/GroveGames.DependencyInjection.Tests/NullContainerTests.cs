using GroveGames.DependencyInjection.Caching;

namespace GroveGames.DependencyInjection.Tests;

public class NullContainerTests
{
    [Fact]
    public void Name_ShouldReturnEmptyString()
    {
        var container = new NullContainer();

        var name = container.Name;

        Assert.Equal(string.Empty, name);
    }

    [Fact]
    public void Parent_ShouldReturnNull()
    {
        var container = new NullContainer();

        var parent = container.Parent;

        Assert.Null(parent);
    }

    [Fact]
    public void AddChild_ShouldDoNothing()
    {
        var container = new NullContainer();
        var mockChild = new TestContainer();

        container.AddChild(mockChild);
    }

    [Fact]
    public void RemoveChild_ShouldDoNothing()
    {
        var container = new NullContainer();
        var mockChild = new TestContainer();

        container.RemoveChild(mockChild);
    }

    [Fact]
    public void Dispose_ShouldDoNothing()
    {
        var container = new NullContainer();

        container.Dispose();
    }

    [Fact]
    public void Resolve_ShouldReturnNull()
    {
        var container = new NullContainer();
        var registrationType = typeof(object);

        var result = container.Resolve(registrationType);

        Assert.Null(result);
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
