using MassTransit;
using MongoDB.Bson;
using Moq;

using Usuarios.Domain.Events;

using Usuarios.Infrastructure.Consumer;
using Usuarios.Infrastructure.Interfaces;

namespace Usuarios.Tests.Usuarios.Infrastructure.Consumers
{
    public class CreateUserConsumerTests
    {
        private readonly Mock<IUserReadRepository> _userReadRepositoryMock;
        private readonly Mock<IServiceProvider> _serviceProviderMock;
        private readonly CreateUserConsumer _consumer;

        public CreateUserConsumerTests()
        {
            _userReadRepositoryMock = new Mock<IUserReadRepository>();
            _serviceProviderMock = new Mock<IServiceProvider>();

            _consumer = new CreateUserConsumer(_serviceProviderMock.Object, _userReadRepositoryMock.Object);
        }

        #region Consume_AddsUserToDatabase_WhenEventIsProcessedSuccessfully
        [Fact]
        public async Task Consume_AddsUserToDatabase_WhenEventIsProcessedSuccessfully()
        {
            var message = new UserCreatedEvent(
                id: "123",
                name: "Emilia",
                lastName: "Perez",
                email: "emiliaperez@gmail.com",
                address: "Av. Principal 123",
                phone: "04141234567",
                roleId: "admin"
            );

            var contextMock = new Mock<ConsumeContext<UserCreatedEvent>>();
            contextMock.Setup(c => c.Message).Returns(message);

            _userReadRepositoryMock.Setup(r => r.AddAsync(It.IsAny<BsonDocument>())).Returns(Task.CompletedTask);

            await _consumer.Consume(contextMock.Object);

            _userReadRepositoryMock.Verify(r => r.AddAsync(It.IsAny<BsonDocument>()), Times.Once);
        }
        #endregion

        #region Consume_ThrowsException_WhenRepositoryFails
        [Fact]
        public async Task Consume_ThrowsException_WhenRepositoryFails()
        {
            var message = new UserCreatedEvent(
                id: "123",
                name: "Emilia",
                lastName: "Perez",
                email: "emiliaperez@gmail.com",
                address: "Av. Principal 123",
                phone: "04141234567",
                roleId: "admin"
            );

            var contextMock = new Mock<ConsumeContext<UserCreatedEvent>>();
            contextMock.Setup(c => c.Message).Returns(message);

            _userReadRepositoryMock.Setup(r => r.AddAsync(It.IsAny<BsonDocument>())).ThrowsAsync(new Exception("Database error"));

            await Assert.ThrowsAsync<Exception>(() => _consumer.Consume(contextMock.Object));
        }


        #endregion

        #region Consume_DoesNotThrowException_WhenEventContainsNullValues
        [Fact]
        public async Task Consume_DoesNotThrowException_WhenEventContainsNullValues()
        {
            var message = new UserCreatedEvent(
                id: "123",
                name: "Emilia",
                lastName: "Perez",
                email: "emiliaperez@gmail.com",
                address: null,
                phone: null,
                roleId: "admin"
            );

            var contextMock = new Mock<ConsumeContext<UserCreatedEvent>>();
            contextMock.Setup(c => c.Message).Returns(message);

            _userReadRepositoryMock.Setup(r => r.AddAsync(It.IsAny<BsonDocument>())).Returns(Task.CompletedTask);

            await _consumer.Consume(contextMock.Object);

            _userReadRepositoryMock.Verify(r => r.AddAsync(It.IsAny<BsonDocument>()), Times.Once);
        }
        #endregion

    }
}
