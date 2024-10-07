using DependencyInjection.Activators;
using DependencyInjection.Resolution;
using DependencyInjection.Injectors;

namespace DependencyInjection.Tests.Injectors;

public class MethodBaseInjectorTests
{
    [Fact]
    public void Inject_ShouldResolveAllParametersAndActivateObject()
    {
        // Arrange
        var mockObjectActivator = new Mock<IObjectActivator>();
        var mockRegistrationResolver = new Mock<IRegistrationResolver>();

        var uninitializedObject = new object();
        var parameterTypes = new[] { typeof(string), typeof(int) };
        var resolvedParameters = new object[] { "test-string", 42 };

        // Set up mock for the object activator to return the parameter types
        mockObjectActivator.Setup(oa => oa.ParameterTypes).Returns(parameterTypes);

        // Set up mock for the registration resolver to resolve parameters
        mockRegistrationResolver.Setup(rr => rr.Resolve(typeof(string))).Returns(resolvedParameters[0]);
        mockRegistrationResolver.Setup(rr => rr.Resolve(typeof(int))).Returns(resolvedParameters[1]);

        // Act
        MethodBaseInjector.Inject(uninitializedObject, mockObjectActivator.Object, mockRegistrationResolver.Object);

        // Assert
        // Verify that the registration resolver was called to resolve the string and int types
        mockRegistrationResolver.Verify(rr => rr.Resolve(typeof(string)), Times.Once);
        mockRegistrationResolver.Verify(rr => rr.Resolve(typeof(int)), Times.Once);

        // Verify that the object activator's Activate method was called with the correct parameters
        mockObjectActivator.Verify(oa => oa.Activate(uninitializedObject, resolvedParameters), Times.Once);
    }
}
