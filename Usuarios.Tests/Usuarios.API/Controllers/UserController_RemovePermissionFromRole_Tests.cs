using log4net;
using MassTransit;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;

using Usuarios.Presentation.Controllers;

using Usuarios.Application.Commands;
using Usuarios.Application.DTOs;

namespace Usuarios.Tests.Usuarios.API.Controllers;

public class UserController_RemovePermissionFromRole_Tests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly Mock<IPublishEndpoint> _publishEndpointMock;
    private readonly Mock<ILog> _loggerMock;
    private readonly UsersController _controller;

    public UserController_RemovePermissionFromRole_Tests()
    {
        _mediatorMock = new Mock<IMediator>();
        _publishEndpointMock = new Mock<IPublishEndpoint>();
        _controller = new UsersController(_mediatorMock.Object, _publishEndpointMock.Object);
        _loggerMock = new Mock<ILog>();
    }

    #region RemovePermissionFromRole_ReturnsOk_WhenPermissionRemovedSuccessfully
    [Fact]
    public async Task RemovePermissionFromRole_ReturnsOk_WhenPermissionRemovedSuccessfully()
    {
        var roleId = Guid.NewGuid().ToString();
        var permissionId = Guid.NewGuid().ToString();
        _mediatorMock.Setup(m => m.Send(It.IsAny<RemovePermissionFromRoleCommand>(), default)).ReturnsAsync(true);

        var result = await _controller.RemovePermissionFromRole(roleId, permissionId) as OkObjectResult;

        //Assert.NotNull(result);
        //Assert.Equal(200, result.StatusCode);
        Assert.Equal("Permiso eliminado correctamente del rol.", result.Value);
    }
    #endregion

    #region RemovePermissionFromRole_ReturnsBadRequest_WhenPermissionRemovalFails
    [Fact]
    public async Task RemovePermissionFromRole_ReturnsBadRequest_WhenPermissionRemovalFails()
    {
        var roleId = Guid.NewGuid().ToString();
        var permissionId = Guid.NewGuid().ToString();
        _mediatorMock.Setup(m => m.Send(It.IsAny<RemovePermissionFromRoleCommand>(), default)).ReturnsAsync(false);

        var result = await _controller.RemovePermissionFromRole(roleId, permissionId) as BadRequestObjectResult;

        //Assert.NotNull(result);
        //Assert.Equal(400, result.StatusCode);
        Assert.Equal("Error al eliminar el permiso.", result.Value);
    }
    #endregion

    #region RemovePermissionFromRole_ReturnsInternalServerError_WhenExceptionOccurs
    [Fact]
    public async Task RemovePermissionFromRole_ReturnsInternalServerError_WhenExceptionOccurs()
    {
        var roleId = Guid.NewGuid().ToString();
        var permissionId = Guid.NewGuid().ToString();
        _mediatorMock.Setup(m => m.Send(It.IsAny<RemovePermissionFromRoleCommand>(), default)).ThrowsAsync(new Exception("Database failure"));

        var result = await _controller.RemovePermissionFromRole(roleId, permissionId) as ObjectResult;

        //Assert.NotNull(result);
        //Assert.Equal(500, result.StatusCode);
        Assert.Equal("Error interno en el servidor.", result.Value);
    }
    #endregion

}
