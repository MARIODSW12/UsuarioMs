using MassTransit;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using log4net;

using Usuarios.Presentation.Controllers;

using Usuarios.Application.Commands;
using Usuarios.Application.DTOs;

namespace Usuarios.Tests.Usuarios.API.Controllers;

public class UserController_CreateUser_Tests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly Mock<IPublishEndpoint> _publishEndpointMock;
    private readonly Mock<ILog> _loggerMock;
    private readonly UsersController _controller;

    public UserController_CreateUser_Tests()
    {
        _mediatorMock = new Mock<IMediator>();
        _publishEndpointMock = new Mock<IPublishEndpoint>();
        _controller = new UsersController(_mediatorMock.Object, _publishEndpointMock.Object);
        _loggerMock = new Mock<ILog>();
    }

    #region CreateUser_ReturnsCreatedAtAction_WhenUserIsCreated
    [Fact]
    public async Task CreateUser_ReturnsCreatedAtAction_WhenUserIsCreated()
    {
        var expectedUserId = Guid.NewGuid().ToString();
        _mediatorMock.Setup(m => m.Send(It.IsAny<CreateUserCommand>(), default))
            .ReturnsAsync(expectedUserId);

        var userDto = new CreateUserDto
        {
            Name = "María",
            LastName = "Pérez",
            Email = "maria@example.com",
            RoleId = "123456",
            Address = "Av. Caracas 123",
            Phone = "04123456789"
        };

        var result = await _controller.CreateUser(userDto) as CreatedAtActionResult;

        //Assert.NotNull(result);
        //Assert.Equal(nameof(UsersController.CreateUser), result.ActionName);
        Assert.Equal(expectedUserId, result.RouteValues["id"]);
    }
    #endregion

    #region CreateUser_ReturnsBadRequest_WhenUserIdIsNull
    [Fact]
    public async Task CreateUser_ReturnsBadRequest_WhenUserIdIsNull()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<CreateUserCommand>(), default)).ReturnsAsync((string)null);

        var userDto = new CreateUserDto
        {
            Name = "María",
            LastName = "Pérez",
            Email = "maria@example.com",
            RoleId = "123456",
            Address = "Av. Caracas 123",
            Phone = "04123456789"
        };

        var result = await _controller.CreateUser(userDto) as BadRequestObjectResult;

        //Assert.NotNull(result);
        //Assert.Equal(400, result.StatusCode);
        Assert.Equal("No se pudo crear el usuario.", result.Value);
    }
    #endregion

    #region CreateUser_ReturnsInternalServerError_WhenExceptionOccurs
    [Fact]
    public async Task CreateUser_ReturnsInternalServerError_WhenExceptionOccurs()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<CreateUserCommand>(), default))
            .ThrowsAsync(new Exception("Database failure"));

        var userDto = new CreateUserDto
        {
            Name = "María",
            LastName = "Pérez",
            Email = "maria@example.com",
            RoleId = "123456",
            Address = "Av. Caracas 123",
            Phone = "04123456789"
        };

        var result = await _controller.CreateUser(userDto) as ObjectResult;

        //Assert.NotNull(result);
        //Assert.Equal(500, result.StatusCode);
        Assert.Equal("Error interno en el servidor.", result.Value);
    }
    #endregion

}
