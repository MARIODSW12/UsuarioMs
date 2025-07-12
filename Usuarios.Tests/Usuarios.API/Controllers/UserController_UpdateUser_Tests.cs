using log4net;
using MassTransit;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;

using Usuarios.Presentation.Controllers;

using Usuarios.Application.Commands;
using Usuarios.Application.DTOs;

namespace Usuarios.Tests.Usuarios.API.Controllers;

public class UserController_UpdateUser_Tests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly Mock<IPublishEndpoint> _publishEndpointMock;
    private readonly Mock<ILog> _loggerMock;
    private readonly UsersController _controller;

    public UserController_UpdateUser_Tests()
    {
        _mediatorMock = new Mock<IMediator>();
        _publishEndpointMock = new Mock<IPublishEndpoint>();
        _controller = new UsersController(_mediatorMock.Object, _publishEndpointMock.Object);
        _loggerMock = new Mock<ILog>();
    }

    #region UpdateUser_ReturnsOk_WhenUpdateSucceeds
    [Fact]
    public async Task UpdateUser_ReturnsOk_WhenUpdateSucceeds()
    {
        var userDto = new UpdateUserDto
        {
            Name = "María",
            LastName = "Pérez",
            Address = "Av. Caracas 123",
            Phone = "04123456789"
        };

        var userId = Guid.NewGuid().ToString();
        _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateUserCommand>(), default)).ReturnsAsync(true);

        var result = await _controller.UpdateUser(userId, userDto) as OkObjectResult;

        //Assert.NotNull(result);
        //Assert.Equal(200, result.StatusCode);
        Assert.Equal("Usuario actualizado exitosamente.", result.Value);
    }
    #endregion

    #region UpdateUser_ReturnsNotFound_WhenUserDoesNotExist
    [Fact]
    public async Task UpdateUser_ReturnsNotFound_WhenUserDoesNotExist()
    {
        var userDto = new UpdateUserDto
        {
            Name = "María",
            LastName = "Pérez",
            Address = "Av. Caracas 123",
            Phone = "04123456789"
        };

        var userId = Guid.NewGuid().ToString();
        _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateUserCommand>(), default)).ReturnsAsync(false);

        var result = await _controller.UpdateUser(userId, userDto) as NotFoundObjectResult;

        //Assert.NotNull(result);
        //Assert.Equal(404, result.StatusCode);
        Assert.Equal("El usuario no pudo ser actualizado.", result.Value);
    }
    #endregion

    #region UpdateUser_ReturnsInternalServerError_WhenExceptionOccurs
    [Fact]
    public async Task UpdateUser_ReturnsInternalServerError_WhenExceptionOccurs()
    {
        var userDto = new UpdateUserDto
        {
            Name = "María",
            LastName = "Pérez",
            Address = "Av. Caracas 123",
            Phone = "04123456789"
        };
        var userId = Guid.NewGuid().ToString();
        _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateUserCommand>(), default))
            .ThrowsAsync(new Exception("Database failure"));

        var result = await _controller.UpdateUser(userId, userDto) as ObjectResult;

        //Assert.NotNull(result);
        //Assert.Equal(500, result.StatusCode);
        Assert.Equal("Error interno en el servidor.", result.Value);
    }
    #endregion

}
