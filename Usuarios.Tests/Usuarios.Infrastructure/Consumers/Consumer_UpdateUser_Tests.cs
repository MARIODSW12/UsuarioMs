using MassTransit;
using MongoDB.Bson;
using Moq;

using Usuarios.Domain.Events;
using Usuarios.Domain.ValueObjects;

using Usuarios.Infrastructure.Consumer;
using Usuarios.Infrastructure.Interfaces;

namespace Usuarios.Tests.Usuarios.Infrastructure.Consumers
{
    public class UpdateUserConsumerTests
    {
        private readonly Mock<IUserReadRepository> _userReadRepositoryMock;
        private readonly Mock<IServiceProvider> _serviceProviderMock;
        private readonly UpdateUserConsumer _consumer;

        public UpdateUserConsumerTests()
        {
            _userReadRepositoryMock = new Mock<IUserReadRepository>();
            _serviceProviderMock = new Mock<IServiceProvider>();

            _consumer = new UpdateUserConsumer(_serviceProviderMock.Object, _userReadRepositoryMock.Object);
        }

        #region Consume_UpdatesUserInDatabase_WhenEventIsProcessedSuccessfully
        [Fact]
        public async Task Consume_UpdatesUserInDatabase_WhenEventIsProcessedSuccessfully()
        {
            var message = new UserUpdatedEvent(
                new VOId("550e8400-e29b-41d4-a716-446655440000"),
                new VOName("Emilia"),
                new VOLastName("Perez"),
                new VOAddress("Av. Principal 123"),
                new VOPhone("04141234567")
            );

            var contextMock = new Mock<ConsumeContext<UserUpdatedEvent>>();
            contextMock.Setup(c => c.Message).Returns(message);

            _userReadRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<BsonDocument>())).Returns(Task.CompletedTask);

            await _consumer.Consume(contextMock.Object);

            _userReadRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<BsonDocument>()), Times.Once);
        }
        #endregion

        #region Consume_ThrowsException_WhenRepositoryFails
        [Fact]
        public async Task Consume_ThrowsException_WhenRepositoryFails()
        {
            var message = new UserUpdatedEvent(
                new VOId("550e8400-e29b-41d4-a716-446655440000"),
                new VOName("Emilia"),
                new VOLastName("Perez"),
                new VOAddress("Av. Principal 123"),
                new VOPhone("04141234567")
            );

            var contextMock = new Mock<ConsumeContext<UserUpdatedEvent>>();
            contextMock.Setup(c => c.Message).Returns(message);

            _userReadRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<BsonDocument>())).ThrowsAsync(new Exception("Database error"));

            await Assert.ThrowsAsync<Exception>(() => _consumer.Consume(contextMock.Object));
        }
        #endregion

        #region Consume_DoesNotThrowException_WhenEventContainsNullValues
        [Fact]
        public async Task Consume_DoesNotThrowException_WhenEventContainsNullValues()
        {
            var message = new UserUpdatedEvent(
                new VOId("550e8400-e29b-41d4-a716-446655440000"),
                new VOName("Emilia"),
                new VOLastName("Perez"),
                null, // Dirección nula
                null  // Teléfono nulo
            );

            var contextMock = new Mock<ConsumeContext<UserUpdatedEvent>>();
            contextMock.Setup(c => c.Message).Returns(message);

            _userReadRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<BsonDocument>())).Returns(Task.CompletedTask);

            await _consumer.Consume(contextMock.Object);

            _userReadRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<BsonDocument>()), Times.Once);
        }
        #endregion

    }
}
