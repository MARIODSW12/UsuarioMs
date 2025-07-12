using MassTransit;
using MongoDB.Bson;
using Moq;

using Usuarios.Domain.Events;

using Usuarios.Infrastructure.Consumer;
using Usuarios.Infrastructure.Interfaces;

namespace Usuarios.Tests.Usuarios.Infrastructure.Consumers
{
    public class UserRoleUpdateConsumerTests
    {
        private readonly Mock<IUserReadRepository> _userReadRepositoryMock;
        private readonly Mock<IServiceProvider> _serviceProviderMock;
        private readonly UserRoleUpdateConsumer _consumer;

        public UserRoleUpdateConsumerTests()
        {
            _userReadRepositoryMock = new Mock<IUserReadRepository>();
            _serviceProviderMock = new Mock<IServiceProvider>();

            _consumer = new UserRoleUpdateConsumer(_serviceProviderMock.Object, _userReadRepositoryMock.Object);
        }

        #region Consume_UpdatesUserRoleInDatabase_WhenEventIsProcessedSuccessfully
        [Fact]
        public async Task Consume_UpdatesUserRoleInDatabase_WhenEventIsProcessedSuccessfully()
        {
            var message = new UserRoleUpdatedEvent("user123", "admin");

            var contextMock = new Mock<ConsumeContext<UserRoleUpdatedEvent>>();
            contextMock.Setup(c => c.Message).Returns(message);

            _userReadRepositoryMock.Setup(r => r.UpdateRoleIdById(It.IsAny<BsonDocument>())).Returns(Task.CompletedTask);

            await _consumer.Consume(contextMock.Object);

            _userReadRepositoryMock.Verify(r => r.UpdateRoleIdById(It.Is<BsonDocument>(bson =>
                bson["_id"] == message.UserId &&
                bson["roleId"] == message.NewRoleId
            )), Times.Once);
        }
        #endregion

        #region Consume_ThrowsException_WhenRepositoryFails
        [Fact]
        public async Task Consume_ThrowsException_WhenRepositoryFails()
        {
            var message = new UserRoleUpdatedEvent("user123", "admin");

            var contextMock = new Mock<ConsumeContext<UserRoleUpdatedEvent>>();
            contextMock.Setup(c => c.Message).Returns(message);

            _userReadRepositoryMock.Setup(r => r.UpdateRoleIdById(It.IsAny<BsonDocument>())).ThrowsAsync(new Exception("Database error"));

            await Assert.ThrowsAsync<Exception>(() => _consumer.Consume(contextMock.Object));
        }
        #endregion

        #region Consume_ValidatesUserIdAndRoleId_AfterProcessingEvent
        [Fact]
        public async Task Consume_ValidatesUserIdAndRoleId_AfterProcessingEvent()
        {
            var message = new UserRoleUpdatedEvent("user123", "moderator");

            var contextMock = new Mock<ConsumeContext<UserRoleUpdatedEvent>>();
            contextMock.Setup(c => c.Message).Returns(message);

            _userReadRepositoryMock.Setup(r => r.UpdateRoleIdById(It.IsAny<BsonDocument>())).Returns(Task.CompletedTask);

            await _consumer.Consume(contextMock.Object);

            _userReadRepositoryMock.Verify(r => r.UpdateRoleIdById(It.Is<BsonDocument>(bson =>
                bson["_id"] == "user123" && bson["roleId"] == "moderator"
            )), Times.Once);
        }
        #endregion

    }
}
