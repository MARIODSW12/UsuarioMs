using log4net;
using MassTransit;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Usuarios.Infrastructure.Queries;
using Usuarios.Presentation.Controllers;

namespace Usuarios.Tests.Usuarios.API.Controllers
{
    public class UserController_GetPermissionsFromRole_Tests
    {
        private readonly Mock<IMediator> _mediatorMock;
        private readonly Mock<IPublishEndpoint> _publishEndpointMock;
        private readonly Mock<ILog> _loggerMock;
        private readonly UsersController _controller;

        public UserController_GetPermissionsFromRole_Tests()
        {
            _mediatorMock = new Mock<IMediator>();
            _publishEndpointMock = new Mock<IPublishEndpoint>();
            _controller = new UsersController(_mediatorMock.Object, _publishEndpointMock.Object);
            _loggerMock = new Mock<ILog>();
        }

        #region GetPermissionsFromRole_ReturnsOk_WhenRoleHasPermissions
        [Fact]
        public async Task GetPermissionsFromRole_ReturnsOk_WhenRoleHasPermissions()
        {
            var roleId = "role123";
            var permissions = new List<string> { "perm1", "perm2", "perm3" };

            _mediatorMock.Setup(m => m.Send(It.IsAny<GetPermissionsByRoleIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(permissions);

            var result = await _controller.GetPermissionsFromRole(roleId) as OkObjectResult;

            //Assert.NotNull(result);
            //Assert.Equal(200, result.StatusCode);
            Assert.NotNull(result.Value);
            //Assert.Equal(permissions, result.Value);
        }
        #endregion

        #region GetPermissionsFromRole_ReturnsNotFound_WhenRoleHasNoPermissions
        [Fact]
        public async Task GetPermissionsFromRole_ReturnsNotFound_WhenRoleHasNoPermissions()
        {
            var roleId = "role123";

            _mediatorMock.Setup(m => m.Send(It.IsAny<GetPermissionsByRoleIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<string>());

            var result = await _controller.GetPermissionsFromRole(roleId) as NotFoundObjectResult;

            //Assert.NotNull(result);
            //Assert.Equal(404, result.StatusCode);
            Assert.Contains("No se encontraron permisos", result.Value.ToString());
        }
        #endregion

        #region GetPermissionsFromRole_ReturnsNotFound_WhenRoleDoesNotExist
        [Fact]
        public async Task GetPermissionsFromRole_ReturnsNotFound_WhenRoleDoesNotExist()
        {
            var roleId = "nonexistent_role";

            _mediatorMock.Setup(m => m.Send(It.IsAny<GetPermissionsByRoleIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((List<string>?)null);

            var result = await _controller.GetPermissionsFromRole(roleId) as NotFoundObjectResult;

            //Assert.NotNull(result);
            //Assert.Equal(404, result.StatusCode);
            Assert.Contains("No se encontraron permisos", result.Value.ToString());
        }
        #endregion

        #region GetPermissionsFromRole_ThrowsException_WhenMediatRFails
        [Fact]
        public async Task GetPermissionsFromRole_ThrowsException_WhenMediatRFails()
        {
            var roleId = "role123";

            _mediatorMock.Setup(m => m.Send(It.IsAny<GetPermissionsByRoleIdQuery>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Database error"));

            await Assert.ThrowsAsync<Exception>(() => _controller.GetPermissionsFromRole(roleId));
        }
        #endregion

    }
}
