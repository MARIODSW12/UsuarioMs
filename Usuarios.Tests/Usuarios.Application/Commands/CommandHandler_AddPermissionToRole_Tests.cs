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
    public class AddPermissionToRoleCommandHandlerTests
    {
        private readonly Mock<IRoleRepository> _roleRepositoryMock;
        private readonly Mock<IPublishEndpoint> _publishEndpointMock;
        private readonly AddPermissionToRoleCommandHandler _handler;

        public AddPermissionToRoleCommandHandlerTests()
        {
            _roleRepositoryMock = new Mock<IRoleRepository>();
            _publishEndpointMock = new Mock<IPublishEndpoint>();

            _handler = new AddPermissionToRoleCommandHandler(_roleRepositoryMock.Object, _publishEndpointMock.Object);
        }

        #region Handle_ReturnsTrue_WhenPermissionAddedSuccessfully
        [Fact]
        public async Task Handle_ReturnsTrue_WhenPermissionAddedSuccessfully()
        {
            var command = new AddPermissionToRoleCommand("admin", "perm456");
            var role = new Role(new VORoleId("admin"), new VORoleName("admin"), new VORolePermissions(new List<string> { "1", "2" }));

            _roleRepositoryMock.Setup(r => r.GetByIdAsync(command.RoleId)).ReturnsAsync(role);
            _roleRepositoryMock.Setup(r => r.AddPermissionsAsync(It.IsAny<VORoleId>(), It.IsAny<VORolePermissions>()))
                .ReturnsAsync(true);
            _publishEndpointMock.Setup(p => p.Publish(It.IsAny<PermissionAddedToRoleEvent>(), default)).Returns(Task.CompletedTask);

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.True(result);
        }
        #endregion

        #region Handle_ThrowsKeyNotFoundException_WhenRoleDoesNotExist
        [Fact]
        public async Task Handle_ThrowsKeyNotFoundException_WhenRoleDoesNotExist()
        {
            var command = new AddPermissionToRoleCommand("invalidRole", "perm456");

            _roleRepositoryMock.Setup(r => r.GetByIdAsync(command.RoleId)).ReturnsAsync((Role)null);

            await Assert.ThrowsAsync<KeyNotFoundException>(() => _handler.Handle(command, CancellationToken.None));
        }
        #endregion

        #region Handle_ThrowsException_WhenSavingPermissionsFails
        [Fact]
        public async Task Handle_ThrowsException_WhenSavingPermissionsFails()
        {
            var command = new AddPermissionToRoleCommand("admin", "perm456");
            var role = new Role(new VORoleId("admin"), new VORoleName("admin"), new VORolePermissions(new List<string> { "1", "2" }));

            _roleRepositoryMock.Setup(r => r.GetByIdAsync(command.RoleId)).ReturnsAsync(role);
            _roleRepositoryMock.Setup(r => r.AddPermissionsAsync(role.Id, It.IsAny<VORolePermissions>()))
                .ThrowsAsync(new Exception("Database failure"));

            await Assert.ThrowsAsync<Exception>(() => _handler.Handle(command, CancellationToken.None));
        }
        #endregion

        #region Handle_ThrowsException_WhenEventPublishingFails
        [Fact]
        public async Task Handle_ThrowsException_WhenEventPublishingFails()
        {
            var command = new AddPermissionToRoleCommand("admin", "perm456");

            var role = new Role(
                new VORoleId("admin"),
                new VORoleName("admin"),
                new VORolePermissions(new List<string> { "perm123", "perm456" }) // ✅ Corrección aquí
            );

            _roleRepositoryMock.Setup(r => r.GetByIdAsync(command.RoleId)).ReturnsAsync(role);
            _roleRepositoryMock.Setup(r => r.AddPermissionsAsync(It.IsAny<VORoleId>(), It.IsAny<VORolePermissions>()))
                .ReturnsAsync(true);
            _publishEndpointMock.Setup(p => p.Publish(It.IsAny<PermissionAddedToRoleEvent>(), default))
                .ThrowsAsync(new Exception("RabbitMQ error"));

            await Assert.ThrowsAsync<Exception>(() => _handler.Handle(command, CancellationToken.None));
        }
        #endregion

    }
}
