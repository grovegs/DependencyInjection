using GroveGames.DependencyInjection.Collections;
using GroveGames.DependencyInjection.Resolution;

namespace GroveGames.DependencyInjection.Tests.Resolution;

public class FactoryObjectResolverTests
{
    private class TestClassWithInjectMethod : IDisposable
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

    [Fact]
    public void Resolve_ShouldInvokeFactory()
    {
        // Arrange
        var mockImplementation = new TestClassWithInjectMethod();
        var mockRegistrationResolver = new Mock<IRegistrationResolver>();
        var mockDisposableCollection = new Mock<IDisposableCollection>();
        var factory = new Func<object>(() => mockImplementation);
        var objectResolver = new FactoryObjectResolver(
            factory,
            mockRegistrationResolver.Object,
            mockDisposableCollection.Object
        );

        // Act
        var result = objectResolver.Resolve();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(mockImplementation, result);
    }

    [Fact]
    public void Resolve_ShouldInjectDependencies()
    {
        // Arrange
        var mockImplementation = new TestClassWithInjectMethod();
        var mockRegistrationResolver = new Mock<IRegistrationResolver>();
        mockRegistrationResolver.Setup(r => r.Resolve(It.IsAny<Type>())).Returns("InjectedString");
        var mockDisposableCollection = new Mock<IDisposableCollection>();
        var factory = new Func<object>(() => mockImplementation);
        var objectResolver = new FactoryObjectResolver(
            factory,
            mockRegistrationResolver.Object,
            mockDisposableCollection.Object
        );

        // Act
        objectResolver.Resolve();

        // Assert
        Assert.True(mockImplementation.Initialized);
        Assert.Equal("InjectedString", mockImplementation.StringValue);
    }

    [Fact]
    public void Resolve_ShouldAddToDisposableCollection()
    {
        // Arrange
        var mockImplementation = new TestClassWithInjectMethod();
        var mockRegistrationResolver = new Mock<IRegistrationResolver>();
        var mockDisposableCollection = new Mock<IDisposableCollection>();
        var factory = new Func<object>(() => mockImplementation);
        var objectResolver = new FactoryObjectResolver(
            factory,
            mockRegistrationResolver.Object,
            mockDisposableCollection.Object
        );

        // Act
        objectResolver.Resolve();

        // Assert
        mockDisposableCollection.Verify(d => d.TryAdd(mockImplementation), Times.Once);
    }
}