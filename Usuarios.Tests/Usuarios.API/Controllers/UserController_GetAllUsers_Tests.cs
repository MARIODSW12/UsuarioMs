using log4net;
using MassTransit;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Usuarios.Application.Commands;
using Usuarios.Application.DTOs;
using Usuarios.Infrastructure.Queries;
using Usuarios.Presentation.Controllers;

namespace Usuarios.Tests.Usuarios.API.Controllers;

public class UserController_GetAllUsers_Tests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly Mock<IPublishEndpoint> _publishEndpointMock;
    private readonly Mock<ILog> _loggerMock;
    private readonly UsersController _controller;

    public UserController_GetAllUsers_Tests()
    {
        _mediatorMock = new Mock<IMediator>();
        _publishEndpointMock = new Mock<IPublishEndpoint>();
        _controller = new UsersController(_mediatorMock.Object, _publishEndpointMock.Object);
        _loggerMock = new Mock<ILog>();
    }

    #region GetAllUsers_ReturnsOk_WhenUsersExist
    [Fact]
    public async Task GetAllUsers_ReturnsOk_WhenUsersExist()
    {
        var users = new List<UserDto>
        {
            new UserDto { Id = Guid.NewGuid().ToString(), Name = "John Doe", Email = "john@example.com" },
            new UserDto { Id = Guid.NewGuid().ToString(), Name = "Jane Doe", Email = "jane@example.com" }
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetAllUsersQuery>(), default)).ReturnsAsync(users);

        var result = await _controller.GetAllUsers() as OkObjectResult;

        //Assert.NotNull(result);
        //Assert.Equal(200, result.StatusCode);
        Assert.Equal(users, result.Value);
    }
    #endregion

    #region GetAllUsers_ReturnsNotFound_WhenNoUsersExist
    [Fact]
    public async Task GetAllUsers_ReturnsNotFound_WhenNoUsersExist()
    {
        var emptyUsers = new List<UserDto>();
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetAllUsersQuery>(), default)).ReturnsAsync(emptyUsers);

        var result = await _controller.GetAllUsers() as NotFoundObjectResult;

        //Assert.NotNull(result);
        //Assert.Equal(404, result.StatusCode);
        Assert.Equal("No se encontraron usuarios.", result.Value);
    }
    #endregion

    #region GetAllUsers_ReturnsInternalServerError_WhenExceptionOccurs
    [Fact]
    public async Task GetAllUsers_ReturnsInternalServerError_WhenExceptionOccurs()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetAllUsersQuery>(), default)).ThrowsAsync(new Exception("Database failure"));

        var result = await _controller.GetAllUsers() as ObjectResult;

        //Assert.NotNull(result);
        //Assert.Equal(500, result.StatusCode);
        Assert.Equal("Error interno en el servidor.", result.Value);
    }
    #endregion

}
