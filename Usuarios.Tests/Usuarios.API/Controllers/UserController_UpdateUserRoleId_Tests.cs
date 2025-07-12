using log4net;
using MassTransit;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;

using Usuarios.Presentation.Controllers;

using Usuarios.Application.Commands;
using Usuarios.Application.DTOs;

namespace Usuarios.Tests.Usuarios.API.Controllers;

public class UserController_UpdateUserRoleId_Tests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly Mock<IPublishEndpoint> _publishEndpointMock;
    private readonly Mock<ILog> _loggerMock;
    private readonly UsersController _controller;

    public UserController_UpdateUserRoleId_Tests()
    {
        _mediatorMock = new Mock<IMediator>();
        _publishEndpointMock = new Mock<IPublishEndpoint>();
        _controller = new UsersController(_mediatorMock.Object, _publishEndpointMock.Object);
        _loggerMock = new Mock<ILog>();
    }

    #region UpdateUserRole_ReturnsOk_WhenUpdateSucceeds
    [Fact]
    public async Task UpdateUserRole_ReturnsOk_WhenUpdateSucceeds()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var updateUserRoleDto = new UpdateUserRoleDto { NewRoleId = "admin" };
        _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateUserRoleCommand>(), default)).ReturnsAsync(true);

        // Act
        var result = await _controller.UpdateUserRole(userId, updateUserRoleDto) as OkObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
        Assert.Equal("El rol del usuario se actualizó correctamente.", result.Value);
    }
    #endregion

    #region UpdateUserRole_ReturnsBadRequest_WhenUpdateFails
    [Fact]
    public async Task UpdateUserRole_ReturnsBadRequest_WhenUpdateFails()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var updateUserRoleDto = new UpdateUserRoleDto { NewRoleId = "guest" };
        _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateUserRoleCommand>(), default)).ReturnsAsync(false);

        // Act
        var result = await _controller.UpdateUserRole(userId, updateUserRoleDto) as BadRequestObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(400, result.StatusCode);
        Assert.Equal("No se pudo actualizar el rol del usuario.", result.Value);
    }
    #endregion

    #region UpdateUserRole_ReturnsInternalServerError_WhenExceptionOccurs
    [Fact]
    public async Task UpdateUserRole_ReturnsInternalServerError_WhenExceptionOccurs()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var updateUserRoleDto = new UpdateUserRoleDto { NewRoleId = "superuser" };
        _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateUserRoleCommand>(), default)).ThrowsAsync(new Exception("Database failure"));

        // Act
        var result = await _controller.UpdateUserRole(userId, updateUserRoleDto) as ObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(500, result.StatusCode);
        Assert.Equal("Error interno en el servidor.", result.Value);
    }
    #endregion

}
