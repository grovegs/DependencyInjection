using DependencyInjection.Collections;

namespace DependencyInjection.Tests.Collections
{
    public class DisposableCollectionTests
    {
        [Fact]
        public void TryAdd_AddsDisposableObjectToCollection()
        {
            // Arrange
            var disposableMock = new Mock<IDisposable>();
            var collection = new DisposableCollection();

            // Act
            collection.TryAdd(disposableMock.Object);

            // Assert
            Assert.NotEmpty(collection);
        }

        [Fact]
        public void TryAdd_DoesNotAddNonDisposableObjectToCollection()
        {
            // Arrange
            var nonDisposableObject = new object();
            var collection = new DisposableCollection();

            // Act
            collection.TryAdd(nonDisposableObject);

            // Assert
            Assert.Empty(collection);
        }

        [Fact]
        public void Dispose_DisposesAllDisposablesInCollection()
        {
            // Arrange
            var disposableMock1 = new Mock<IDisposable>();
            var disposableMock2 = new Mock<IDisposable>();
            var collection = new DisposableCollection();
            collection.TryAdd(disposableMock1.Object);
            collection.TryAdd(disposableMock2.Object);

            // Act
            collection.Dispose();

            // Assert
            disposableMock1.Verify(d => d.Dispose(), Times.Once);
            disposableMock2.Verify(d => d.Dispose(), Times.Once);
        }

        [Fact]
        public void Dispose_CanBeCalledMultipleTimes()
        {
            // Arrange
            var disposableMock = new Mock<IDisposable>();
            var collection = new DisposableCollection();
            collection.TryAdd(disposableMock.Object);

            // Act
            collection.Dispose();
            collection.Dispose();

            // Assert
            disposableMock.Verify(d => d.Dispose(), Times.Once);
        }
    }
}
