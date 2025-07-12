using MassTransit;
using MongoDB.Bson;
using Moq;

using Usuarios.Domain.Events;

using Usuarios.Infrastructure.Consumer;
using Usuarios.Infrastructure.Interfaces;


namespace Usuarios.Tests.Usuarios.Infrastructure.Consumers
{
    public class PermissionAddedToRoleConsumerTests
    {
        private readonly Mock<IRoleReadRepository> _roleReadRepositoryMock;
        private readonly Mock<IServiceProvider> _serviceProviderMock;
        private readonly PermissionAddedToRoleConsumer _consumer;

        public PermissionAddedToRoleConsumerTests()
        {
            _roleReadRepositoryMock = new Mock<IRoleReadRepository>();
            _serviceProviderMock = new Mock<IServiceProvider>();

            _consumer = new PermissionAddedToRoleConsumer(_serviceProviderMock.Object, _roleReadRepositoryMock.Object);
        }

        #region Consume_AddsPermissionToRole_WhenEventIsProcessedSuccessfully
        [Fact]
        public async Task Consume_AddsPermissionToRole_WhenEventIsProcessedSuccessfully()
        {
            var message = new PermissionAddedToRoleEvent("role123", "perm456", DateTime.UtcNow);

            var contextMock = new Mock<ConsumeContext<PermissionAddedToRoleEvent>>();
            contextMock.Setup(c => c.Message).Returns(message);

            _roleReadRepositoryMock.Setup(r => r.AddPermissionsAsync(It.IsAny<BsonDocument>()))
                .ReturnsAsync(true);

            await _consumer.Consume(contextMock.Object);

            _roleReadRepositoryMock.Verify(r => r.AddPermissionsAsync(It.Is<BsonDocument>(bson =>
                bson["_id"] == message.RoleId &&
                bson["PermissionIds"] == message.PermissionId
            )), Times.Once);
        }
        #endregion

        #region Consume_ThrowsException_WhenRepositoryFails
        [Fact]
        public async Task Consume_ThrowsException_WhenRepositoryFails()
        {
            var message = new PermissionAddedToRoleEvent("role123", "perm456", DateTime.UtcNow);

            var contextMock = new Mock<ConsumeContext<PermissionAddedToRoleEvent>>();
            contextMock.Setup(c => c.Message).Returns(message);

            _roleReadRepositoryMock.Setup(r => r.AddPermissionsAsync(It.IsAny<BsonDocument>()))
                .ThrowsAsync(new Exception("Database error"));

            await Assert.ThrowsAsync<Exception>(() => _consumer.Consume(contextMock.Object));
        }
        #endregion

        #region Consume_ValidatesRoleIdAndPermissionId_AfterProcessingEvent
        [Fact]
        public async Task Consume_ValidatesRoleIdAndPermissionId_AfterProcessingEvent()
        {
            var message = new PermissionAddedToRoleEvent("role789", "perm999", DateTime.UtcNow);

            var contextMock = new Mock<ConsumeContext<PermissionAddedToRoleEvent>>();
            contextMock.Setup(c => c.Message).Returns(message);

            _roleReadRepositoryMock.Setup(r => r.AddPermissionsAsync(It.IsAny<BsonDocument>()))
                .ReturnsAsync(true);

            await _consumer.Consume(contextMock.Object);

            _roleReadRepositoryMock.Verify(r => r.AddPermissionsAsync(It.Is<BsonDocument>(bson =>
                bson["_id"] == "role789" && bson["PermissionIds"] == "perm999"
            )), Times.Once);
        }
        #endregion

    }
}
