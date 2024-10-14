using GroveGames.DependencyInjection.Resolution;

namespace GroveGames.DependencyInjection.Tests.Resolution;

public class InstanceResolverTests
{
    [Fact]
    public void Constructor_ShouldInitializeImplementationInstance()
    {
        // Arrange
        var implementationInstance = new object();

        // Act
        var instanceResolver = new InstanceResolver(implementationInstance);

        // Assert
        var field = typeof(InstanceResolver).GetField("_implementationInstance", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        Assert.NotNull(field);
        var instance = field.GetValue(instanceResolver);
        Assert.Equal(implementationInstance, instance);
    }

    [Fact]
    public void Resolve_ShouldReturnImplementationInstance()
    {
        // Arrange
        var implementationInstance = new object();
        var instanceResolver = new InstanceResolver(implementationInstance);

        // Act
        var resolvedInstance = instanceResolver.Resolve();

        // Assert
        Assert.Equal(implementationInstance, resolvedInstance);
    }
}
