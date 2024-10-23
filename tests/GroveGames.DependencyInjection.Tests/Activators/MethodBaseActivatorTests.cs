using System.Reflection;

namespace GroveGames.DependencyInjection.Activators.Tests;

public class MethodBaseActivatorTests
{
    private class TestClass
    {
        public static string? TestMethod(string input, int number)
        {
            return $"{input} - {number}";
        }
    }

    [Fact]
    public void Constructor_ShouldInitializeParameterTypes()
    {
        // Arrange
        var methodInfo = typeof(TestClass).GetMethod(nameof(TestClass.TestMethod));
        Assert.NotNull(methodInfo);

        // Act
        var activator = new MethodBaseActivator(methodInfo);

        // Assert
        Assert.Equal([typeof(string), typeof(int)], activator.ParameterTypes);
    }

    [Fact]
    public void Activate_ShouldInvokeMethodOnUninitializedObject()
    {
        // Arrange
        var methodInfo = typeof(TestClass).GetMethod(nameof(TestClass.TestMethod));
        Assert.NotNull(methodInfo);

        var activator = new MethodBaseActivator(methodInfo!);
        var testClassInstance = new TestClass();
        var parameters = new object[] { "hello", 123 };

        // Act
        activator.Activate(testClassInstance, parameters);

        // Assert
        var result = methodInfo.Invoke(testClassInstance, parameters);
        Assert.Equal("hello - 123", result);
    }

    [Fact]
    public void Activate_ShouldThrowExceptionForInvalidParameterCount()
    {
        // Arrange
        var methodInfo = typeof(TestClass).GetMethod(nameof(TestClass.TestMethod));
        Assert.NotNull(methodInfo);

        var activator = new MethodBaseActivator(methodInfo!);
        var testClassInstance = new TestClass();
        var invalidParameters = new object[] { 123 };

        // Act & Assert
        Assert.Throws<TargetParameterCountException>(() =>
            activator.Activate(testClassInstance, invalidParameters));
    }

    [Fact]
    public void Activate_ShouldThrowExceptionForInvalidParameterOrder()
    {
        // Arrange
        var methodInfo = typeof(TestClass).GetMethod(nameof(TestClass.TestMethod));
        Assert.NotNull(methodInfo);

        var activator = new MethodBaseActivator(methodInfo!);
        var testClassInstance = new TestClass();
        var invalidParameters = new object[] { 123, "wrong order" };

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            activator.Activate(testClassInstance, invalidParameters));
    }
}