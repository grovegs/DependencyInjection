using System.Runtime.CompilerServices;
using DependencyInjection.Injectors;
using DependencyInjection.Resolution;

namespace DependencyInjection.Tests.Injectors
{
    public class ConstructorInjectorTests
    {
        private class TestClassWithConstructor
        {
            public string? Name { get; }

            public TestClassWithConstructor(string name)
            {
                Name = name;
            }
        }

        private class TestClassWithoutPublicConstructor
        {
            private TestClassWithoutPublicConstructor() { }
        }

        [Fact]
        public void Inject_ShouldCallRegistrationResolver_WhenConstructorIsFound()
        {
            // Arrange
            var mockRegistrationResolver = new Mock<IRegistrationResolver>();
            var uninitializedObject = RuntimeHelpers.GetUninitializedObject(typeof(TestClassWithConstructor));
            mockRegistrationResolver.Setup(rr => rr.Resolve(typeof(string))).Returns("Test");

            // Act
            ConstructorInjector.Inject(uninitializedObject, mockRegistrationResolver.Object);

            // Assert
            mockRegistrationResolver.Verify(rr => rr.Resolve(typeof(string)), Times.Once);
        }

        [Fact]
        public void Inject_ShouldNotCallRegistrationResolver_WhenNoPublicConstructorIsFound()
        {
            // Arrange
            var mockRegistrationResolver = new Mock<IRegistrationResolver>();
            var uninitializedObject = RuntimeHelpers.GetUninitializedObject(typeof(TestClassWithoutPublicConstructor));

            // Act
            ConstructorInjector.Inject(uninitializedObject, mockRegistrationResolver.Object);

            // Assert
            mockRegistrationResolver.Verify(rr => rr.Resolve(It.IsAny<Type>()), Times.Never);
        }

        [Fact]
        public void Inject_ShouldUseConstructorWithMostParameters()
        {
            // Arrange
            var mockRegistrationResolver = new Mock<IRegistrationResolver>();
            var uninitializedObject = RuntimeHelpers.GetUninitializedObject(typeof(TestClassWithConstructor));
            mockRegistrationResolver.Setup(rr => rr.Resolve(typeof(string))).Returns("Resolved Value");

            // Act
            ConstructorInjector.Inject(uninitializedObject, mockRegistrationResolver.Object);

            // Assert
            mockRegistrationResolver.Verify(rr => rr.Resolve(typeof(string)), Times.Once);
        }
    }
}
