using MassTransit;
using Moq;

using Usuarios.Domain.Events;
using Usuarios.Infrastructure.Consumer;
using Usuarios.Infrastructure.Interfaces;

namespace Usuarios.Tests.Usuarios.Infrastructure.Consumers
{
    public class PermissionRemovedFromRoleConsumerTests
    {
        private readonly Mock<IRoleReadRepository> _roleReadRepositoryMock;
        private readonly Mock<IServiceProvider> _serviceProviderMock;
        private readonly PermissionRemovedFromRoleConsumer _consumer;

        public PermissionRemovedFromRoleConsumerTests()
        {
            _roleReadRepositoryMock = new Mock<IRoleReadRepository>();
            _serviceProviderMock = new Mock<IServiceProvider>();

            _consumer = new PermissionRemovedFromRoleConsumer(_serviceProviderMock.Object, _roleReadRepositoryMock.Object);
        }

        #region Consume_RemovesPermissionFromRole_WhenEventIsProcessedSuccessfully
        [Fact]
        public async Task Consume_RemovesPermissionFromRole_WhenEventIsProcessedSuccessfully()
        {
            var message = new PermissionRemovedFromRoleEvent("role123", "perm456", DateTime.UtcNow);

            var contextMock = new Mock<ConsumeContext<PermissionRemovedFromRoleEvent>>();
            contextMock.Setup(c => c.Message).Returns(message);

            _roleReadRepositoryMock.Setup(r => r.RemovePermissionByIdAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            await _consumer.Consume(contextMock.Object);

            _roleReadRepositoryMock.Verify(r => r.RemovePermissionByIdAsync(message.RoleId, message.PermissionId), Times.Once);
        }
        #endregion

        #region Consume_ThrowsException_WhenRepositoryFails
        [Fact]
        public async Task Consume_ThrowsException_WhenRepositoryFails()
        {
            var message = new PermissionRemovedFromRoleEvent("role123", "perm456", DateTime.UtcNow);

            var contextMock = new Mock<ConsumeContext<PermissionRemovedFromRoleEvent>>();
            contextMock.Setup(c => c.Message).Returns(message);

            _roleReadRepositoryMock.Setup(r => r.RemovePermissionByIdAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception("Database error"));

            await Assert.ThrowsAsync<Exception>(() => _consumer.Consume(contextMock.Object));
        }
        #endregion

        #region Consume_ValidatesRoleIdAndPermissionId_BeforeProcessingEvent
        [Fact]
        public async Task Consume_ValidatesRoleIdAndPermissionId_BeforeProcessingEvent()
        {
            var message = new PermissionRemovedFromRoleEvent("role789", "perm999", DateTime.UtcNow);

            var contextMock = new Mock<ConsumeContext<PermissionRemovedFromRoleEvent>>();
            contextMock.Setup(c => c.Message).Returns(message);

            _roleReadRepositoryMock.Setup(r => r.RemovePermissionByIdAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            await _consumer.Consume(contextMock.Object);

            _roleReadRepositoryMock.Verify(r => r.RemovePermissionByIdAsync("role789", "perm999"), Times.Once);
        }
        #endregion

    }
}
