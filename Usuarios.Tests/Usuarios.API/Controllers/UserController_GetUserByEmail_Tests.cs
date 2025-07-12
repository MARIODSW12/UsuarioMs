using log4net;
using MassTransit;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;

using Usuarios.Presentation.Controllers;

using Usuarios.Application.DTOs;

using Usuarios.Infrastructure.Queries;

namespace Usuarios.Tests.Usuarios.API.Controllers;

public class UserController_GetUserByEmail_Tests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly Mock<IPublishEndpoint> _publishEndpointMock;
    private readonly Mock<ILog> _loggerMock;
    private readonly UsersController _controller;

    public UserController_GetUserByEmail_Tests()
    {
        _mediatorMock = new Mock<IMediator>();
        _publishEndpointMock = new Mock<IPublishEndpoint>();
        _controller = new UsersController(_mediatorMock.Object, _publishEndpointMock.Object);
        _loggerMock = new Mock<ILog>();
    }

    #region GetUserByEmail_ReturnsOk_WhenUserExists
    [Fact]
    public async Task GetUserByEmail_ReturnsOk_WhenUserExists()
    {
        var email = "test@example.com";
        var userByEmailDto = new UserByEmailDto { userId = Guid.NewGuid().ToString(), Email = email, Name = "John Doe" };

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetUserByEmailQuery>(), default)).ReturnsAsync(userByEmailDto);

        var result = await _controller.GetUserByEmail(email) as OkObjectResult;

        //Assert.NotNull(result);
        //Assert.Equal(200, result.StatusCode);
        Assert.Equal(userByEmailDto, result.Value);
    }
    #endregion

    #region GetUserByEmail_ReturnsNotFound_WhenUserDoesNotExist
    [Fact]
    public async Task GetUserByEmail_ReturnsNotFound_WhenUserDoesNotExist()
    {
        var email = "nonexistent@example.com";
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetUserByEmailQuery>(), default)).ReturnsAsync((UserByEmailDto)null);

        var result = await _controller.GetUserByEmail(email) as NotFoundObjectResult;

        //Assert.NotNull(result);
        //Assert.Equal(404, result.StatusCode);
        Assert.Equal($"No se encontró un usuario con el email {email}", result.Value);
    }
    #endregion

    #region GetUserByEmail_ReturnsInternalServerError_WhenExceptionOccurs
    [Fact]
    public async Task GetUserByEmail_ReturnsInternalServerError_WhenExceptionOccurs()
    {
        var email = "error@example.com";
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetUserByEmailQuery>(), default)).ThrowsAsync(new Exception("Database failure"));

        var result = await _controller.GetUserByEmail(email) as ObjectResult;

        //Assert.NotNull(result);
        //Assert.Equal(500, result.StatusCode);
        Assert.Equal("Error interno en el servidor.", result.Value);
    }
    #endregion

}
