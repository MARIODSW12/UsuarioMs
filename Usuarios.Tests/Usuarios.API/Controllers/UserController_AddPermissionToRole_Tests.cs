using log4net;
using MassTransit;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;

using Usuarios.Presentation.Controllers;

using Usuarios.Application.Commands;

namespace Usuarios.Tests.Usuarios.API.Controllers;

public class UserController_AddPermissionToRole_Tests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly Mock<IPublishEndpoint> _publishEndpointMock;
    private readonly Mock<ILog> _loggerMock;
    private readonly UsersController _controller;

    public UserController_AddPermissionToRole_Tests()
    {
        _mediatorMock = new Mock<IMediator>();
        _publishEndpointMock = new Mock<IPublishEndpoint>();
        _controller = new UsersController(_mediatorMock.Object, _publishEndpointMock.Object);
        _loggerMock = new Mock<ILog>();
    }

    #region AddPermissionToRole_ReturnsOk_WhenPermissionAddedSuccessfully
    [Fact]
    public async Task AddPermissionToRole_ReturnsOk_WhenPermissionAddedSuccessfully()
    {
        var roleId = Guid.NewGuid().ToString();
        var permissionId = Guid.NewGuid().ToString();
        _mediatorMock.Setup(m => m.Send(It.IsAny<AddPermissionToRoleCommand>(), default)).ReturnsAsync(true);

        var result = await _controller.AddPermissionToRole(roleId, permissionId) as OkObjectResult;

        //Assert.NotNull(result);
        //Assert.Equal(200, result.StatusCode);
        Assert.Equal("Permiso agregado correctamente al rol.", result.Value);
    }
    #endregion

    #region AddPermissionToRole_ReturnsBadRequest_WhenPermissionAdditionFails
    [Fact]
    public async Task AddPermissionToRole_ReturnsBadRequest_WhenPermissionAdditionFails()
    {
        var roleId = Guid.NewGuid().ToString();
        var permissionId = Guid.NewGuid().ToString();
        _mediatorMock.Setup(m => m.Send(It.IsAny<AddPermissionToRoleCommand>(), default)).ReturnsAsync(false);

        var result = await _controller.AddPermissionToRole(roleId, permissionId) as BadRequestObjectResult;

        //Assert.NotNull(result);
        //Assert.Equal(400, result.StatusCode);
        Assert.Equal("Error al agregar el permiso.", result.Value);
    }
    #endregion

    #region AddPermissionToRole_ReturnsInternalServerError_WhenExceptionOccurs
    [Fact]
    public async Task AddPermissionToRole_ReturnsInternalServerError_WhenExceptionOccurs()
    {
        var roleId = Guid.NewGuid().ToString();
        var permissionId = Guid.NewGuid().ToString();
        _mediatorMock.Setup(m => m.Send(It.IsAny<AddPermissionToRoleCommand>(), default)).ThrowsAsync(new Exception("Database failure"));

        var result = await _controller.AddPermissionToRole(roleId, permissionId) as ObjectResult;

        //Assert.NotNull(result);
        //Assert.Equal(500, result.StatusCode);
        Assert.Equal("Error interno en el servidor.", result.Value);
    }
    #endregion

}
