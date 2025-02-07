using GroveGames.DependencyInjection.Resolution;

namespace GroveGames.DependencyInjection.Tests.Resolution;

public class ContainerResolverTests
{
    [Fact]
    public void Resolve_ShouldReturnInstanceFromInstanceResolver()
    {
        // Arrange
        var registrationType = typeof(object);
        var expectedInstance = new object();
        var mockInstanceResolver = new Mock<IInstanceResolver>();
        mockInstanceResolver.Setup(r => r.Resolve()).Returns(expectedInstance);
        var mockParentResolver = new Mock<IObjectResolver>();
        var containerResolver = new ContainerResolver(mockParentResolver.Object);
        containerResolver.AddInstanceResolver(registrationType, mockInstanceResolver.Object);

        // Act
        var resolvedInstance = containerResolver.Resolve(registrationType);

        // Assert
        Assert.Equal(expectedInstance, resolvedInstance);
    }

    [Fact]
    public void Resolve_ShouldReturnInstanceFromParentResolver_WhenInstanceResolverNotFound()
    {
        // Arrange
        var registrationType = typeof(object);
        var expectedInstance = new object();
        var mockParentResolver = new Mock<IObjectResolver>();
        mockParentResolver.Setup(r => r.Resolve(registrationType)).Returns(expectedInstance);
        var containerResolver = new ContainerResolver(mockParentResolver.Object);

        // Act
        var resolvedInstance = containerResolver.Resolve(registrationType);

        // Assert
        Assert.Equal(expectedInstance, resolvedInstance);
    }

    [Fact]
    public void AddInstanceResolver_ShouldAddInstanceResolver()
    {
        // Arrange
        var registrationType = typeof(object);
        var mockInstanceResolver = new Mock<IInstanceResolver>();
        var mockParentResolver = new Mock<IObjectResolver>();
        var containerResolver = new ContainerResolver(mockParentResolver.Object);

        // Act
        containerResolver.AddInstanceResolver(registrationType, mockInstanceResolver.Object);

        // Assert
        var field = typeof(ContainerResolver).GetField("_instanceResolversByRegistrationTypes", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        Assert.NotNull(field);
        var instanceResolvers = field.GetValue(containerResolver) as Dictionary<Type, IInstanceResolver>;
        Assert.NotNull(instanceResolvers);
        Assert.True(instanceResolvers.ContainsKey(registrationType));
    }

    [Fact]
    public void Clear_ShouldRemoveAllInstanceResolvers()
    {
        // Arrange
        var registrationType = typeof(object);
        var mockInstanceResolver = new Mock<IInstanceResolver>();
        var mockParentResolver = new Mock<IObjectResolver>();
        var containerResolver = new ContainerResolver(mockParentResolver.Object);
        containerResolver.AddInstanceResolver(registrationType, mockInstanceResolver.Object);

        // Act
        containerResolver.Clear();

        // Assert
        var field = typeof(ContainerResolver).GetField("_instanceResolversByRegistrationTypes", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        Assert.NotNull(field);
        var instanceResolvers = field.GetValue(containerResolver) as Dictionary<Type, IInstanceResolver>;
        Assert.NotNull(instanceResolvers);
        Assert.Empty(instanceResolvers);
    }
}