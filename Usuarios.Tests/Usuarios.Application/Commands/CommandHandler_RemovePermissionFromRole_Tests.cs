using MassTransit;
using Moq;

using Usuarios.Application.Commands;
using Usuarios.Application.Handlers;

using Usuarios.Domain.Entities;
using Usuarios.Domain.Events;
using Usuarios.Domain.Repositories;
using Usuarios.Domain.ValueObjects;

namespace Usuarios.Tests.Usuarios.Application.Commands
{
    public class RemovePermissionFromRoleCommandHandlerTests
    {
        private readonly Mock<IRoleRepository> _roleRepositoryMock;
        private readonly Mock<IPublishEndpoint> _publishEndpointMock;
        private readonly RemovePermissionFromRoleCommandHandler _handler;

        public RemovePermissionFromRoleCommandHandlerTests()
        {
            _roleRepositoryMock = new Mock<IRoleRepository>();
            _publishEndpointMock = new Mock<IPublishEndpoint>();

            _handler = new RemovePermissionFromRoleCommandHandler(_roleRepositoryMock.Object,
                _publishEndpointMock.Object);
        }

        #region Handle_ReturnsTrue_WhenPermissionRemovedSuccessfully
        [Fact]
        public async Task Handle_ReturnsTrue_WhenPermissionRemovedSuccessfully()
        {
            var command = new RemovePermissionFromRoleCommand("admin", "perm456");
            var role = new Role(new VORoleId("admin"), new VORoleName("admin"),
                new VORolePermissions(new List<string> { "perm123", "perm456" }));

            _roleRepositoryMock.Setup(r => r.GetByIdAsync(command.RoleId)).ReturnsAsync(role);
            _roleRepositoryMock.Setup(r => r.UpdateAsync(role)).ReturnsAsync(true);
            _publishEndpointMock.Setup(p => p.Publish(It.IsAny<PermissionRemovedFromRoleEvent>(), default))
                .Returns(Task.CompletedTask);

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.True(result);
        }
        #endregion

        #region Handle_ThrowsKeyNotFoundException_WhenRoleDoesNotExist
        [Fact]
        public async Task Handle_ThrowsKeyNotFoundException_WhenRoleDoesNotExist()
        {
            var command = new RemovePermissionFromRoleCommand("invalidRole", "perm456");

            _roleRepositoryMock.Setup(r => r.GetByIdAsync(command.RoleId)).ReturnsAsync((Role)null);

            await Assert.ThrowsAsync<KeyNotFoundException>(() => _handler.Handle(command, CancellationToken.None));
        }
        #endregion

        #region Handle_ThrowsException_WhenUpdatingRoleFails
        [Fact]
        public async Task Handle_ThrowsException_WhenUpdatingRoleFails()
        {
            var command = new RemovePermissionFromRoleCommand("admin", "perm456");
            var role = new Role(new VORoleId("admin"), new VORoleName("admin"),
                new VORolePermissions(new List<string> { "perm123", "perm456" }));

            _roleRepositoryMock.Setup(r => r.GetByIdAsync(command.RoleId)).ReturnsAsync(role);
            _roleRepositoryMock.Setup(r => r.UpdateAsync(role)).ThrowsAsync(new Exception("Database failure"));

            await Assert.ThrowsAsync<Exception>(() => _handler.Handle(command, CancellationToken.None));
        }
        #endregion

        #region Handle_ThrowsException_WhenEventPublishingFails
        [Fact]
        public async Task Handle_ThrowsException_WhenEventPublishingFails()
        {
            var command = new RemovePermissionFromRoleCommand("admin", "perm456");
            var role = new Role(new VORoleId("admin"), new VORoleName("admin"),
                new VORolePermissions(new List<string> { "perm123", "perm456" }));

            _roleRepositoryMock.Setup(r => r.GetByIdAsync(command.RoleId)).ReturnsAsync(role);
            _roleRepositoryMock.Setup(r => r.UpdateAsync(role)).ReturnsAsync(true);
            _publishEndpointMock.Setup(p => p.Publish(It.IsAny<PermissionRemovedFromRoleEvent>(), default))
                .ThrowsAsync(new Exception("RabbitMQ error"));

            await Assert.ThrowsAsync<Exception>(() => _handler.Handle(command, CancellationToken.None));
        }
        #endregion

    }
}
