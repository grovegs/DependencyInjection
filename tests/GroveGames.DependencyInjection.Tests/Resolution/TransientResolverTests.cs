using GroveGames.DependencyInjection.Resolution;

namespace GroveGames.DependencyInjection.Tests.Resolution
{
    public class TransientResolverTests
    {
        [Fact]
        public void Resolve_ShouldCallObjectResolverResolve()
        {
            // Arrange
            var mockObjectResolver = new Mock<IObjectResolver>();
            var transientResolver = new TransientResolver(mockObjectResolver.Object);
            var expectedObject = new object();
            mockObjectResolver.Setup(r => r.Resolve()).Returns(expectedObject);

            // Act
            var resolvedObject = transientResolver.Resolve();

            // Assert
            mockObjectResolver.Verify(r => r.Resolve(), Times.Once);
            Assert.Equal(expectedObject, resolvedObject);
        }
    }
}
