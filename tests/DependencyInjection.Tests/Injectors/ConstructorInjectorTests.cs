using DependencyInjection.Resolution;
using DependencyInjection.Injectors;

namespace DependencyInjection.Tests.Injectors;

public class ConstructorInjectorTests
{
    private readonly Mock<IRegistrationResolver> _mockRegistrationResolver;

    public ConstructorInjectorTests()
    {
        _mockRegistrationResolver = new Mock<IRegistrationResolver>();
    }

    [Fact]
    public void Inject_WithValidObject_PerformsInjection()
    {
        // Arrange
        var sampleObject = Activator.CreateInstance(typeof(SampleClass));

        // Act
        ConstructorInjector.Inject(sampleObject!, _mockRegistrationResolver.Object);

        // Assert
        var obj = sampleObject as SampleClass;
        Assert.NotNull(obj);
        Assert.True(obj!.IsInjected);
    }

    [Fact]
    public void Inject_WithObjectWithoutPublicConstructor_DoesNotPerformInjection()
    {
        // Arrange
        var invalidObject = Activator.CreateInstance(typeof(InvalidSampleClass), true);

        // Act
        ConstructorInjector.Inject(invalidObject!, _mockRegistrationResolver.Object);

        // Assert
        var obj = invalidObject as InvalidSampleClass;
        Assert.NotNull(obj);
        Assert.False(obj!.IsInjected);
    }

    public class SampleClass
    {
        public bool IsInjected { get; private set; }

        public SampleClass()
        {
            IsInjected = true;
        }
    }

    public class InvalidSampleClass
    {
        public bool IsInjected { get; private set; }

        private InvalidSampleClass()
        {
            IsInjected = false;
        }
    }
}
