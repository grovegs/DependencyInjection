using GroveGames.DependencyInjection.Injectors;
using GroveGames.DependencyInjection.Resolution;

namespace GroveGames.DependencyInjection.Tests.Injectors;

public class MethodInjectorTests
{
    private class TestClassWithInjectMethod
    {
        public bool MethodCalled { get; private set; } = false;
        public string? StringValue { get; private set; }

        [Inject]
        public void Initialize(string value)
        {
            MethodCalled = true;
            StringValue = value;
        }
    }


    private class TestClassWithMultipleInjectMethods
    {
        public string? StringValue { get; private set; }
        public int IntValue { get; private set; }

        [Inject]
        public void InjectMethod1(string value)
        {
            StringValue = value;
        }

        [Inject]
        public void InjectMethod2(string value, int number)
        {
            StringValue = value;
            IntValue = number;
        }
    }

    private class TestClassWithoutInjectMethod
    {
        public void RegularMethod() { }
    }

    [Fact]
    public void Inject_ShouldCallMethodBaseInjector_WhenInjectMethodIsFound()
    {
        // Arrange
        var mockRegistrationResolver = new Mock<IRegistrationResolver>();
        var testObject = new TestClassWithInjectMethod();
        mockRegistrationResolver.Setup(rr => rr.Resolve(typeof(string))).Returns("ResolvedValue");

        // Act
        MethodInjector.Inject(testObject, mockRegistrationResolver.Object);

        // Assert
        Assert.True(testObject.MethodCalled);
        Assert.Equal("ResolvedValue", testObject.StringValue);
        mockRegistrationResolver.Verify(rr => rr.Resolve(typeof(string)), Times.Once);
    }

    [Fact]
    public void Inject_ShouldNotCallMethodBaseInjector_WhenNoInjectMethodIsFound()
    {
        // Arrange
        var mockRegistrationResolver = new Mock<IRegistrationResolver>();
        var testObject = new TestClassWithoutInjectMethod();

        // Act
        MethodInjector.Inject(testObject, mockRegistrationResolver.Object);

        // Assert
        mockRegistrationResolver.Verify(rr => rr.Resolve(It.IsAny<Type>()), Times.Never);
    }

    [Fact]
    public void Inject_ShouldSelectMethodWithMostParameters_WhenMultipleInjectMethodsExist()
    {
        // Arrange
        var testObject = new TestClassWithMultipleInjectMethods();
        var mockRegistrationResolver = new Mock<IRegistrationResolver>();
        mockRegistrationResolver.Setup(rr => rr.Resolve(typeof(string))).Returns("StringValue");
        mockRegistrationResolver.Setup(rr => rr.Resolve(typeof(int))).Returns(42);

        // Act
        MethodInjector.Inject(testObject, mockRegistrationResolver.Object);

        // Assert
        Assert.Equal("StringValue", testObject.StringValue);
        Assert.Equal(42, testObject.IntValue);
        mockRegistrationResolver.Verify(rr => rr.Resolve(typeof(string)), Times.Once);
        mockRegistrationResolver.Verify(rr => rr.Resolve(typeof(int)), Times.Once);
    }
}