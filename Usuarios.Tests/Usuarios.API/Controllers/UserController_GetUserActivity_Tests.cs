using log4net;
using MassTransit;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;

using Usuarios.Presentation.Controllers;

using Usuarios.Application.DTOs;

using Usuarios.Infrastructure.Queries;

namespace Usuarios.Tests.Usuarios.API.Controllers;

public class UserController_GetUserActivity_Tests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly Mock<IPublishEndpoint> _publishEndpointMock;
    private readonly Mock<ILog> _loggerMock;
    private readonly UsersController _controller;

    public UserController_GetUserActivity_Tests()
    {
        _mediatorMock = new Mock<IMediator>();
        _publishEndpointMock = new Mock<IPublishEndpoint>();
        _controller = new UsersController(_mediatorMock.Object, _publishEndpointMock.Object);
        _loggerMock = new Mock<ILog>();
    }

    #region GetByUserIdAsync_ReturnsOk_WhenUserHasActivity
    [Fact]
    public async Task GetByUserIdAsync_ReturnsOk_WhenUserHasActivity()
    {
        var userId = Guid.NewGuid().ToString();
        var activities = new List<UserActivityDto>
        {
            new UserActivityDto { UserId = userId, Action = "Login", Timestamp = DateTime.UtcNow }
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<UserActivityQuery>(), default)).ReturnsAsync(activities);

        var result = await _controller.GetByUserIdAsync(userId) as OkObjectResult;

        //Assert.NotNull(result);
        //Assert.Equal(200, result.StatusCode);
        Assert.Equal(activities, result.Value);
    }
    #endregion

    #region GetByUserIdAsync_ReturnsNotFound_WhenUserHasNoActivity
    [Fact]
    public async Task GetByUserIdAsync_ReturnsNotFound_WhenUserHasNoActivity()
    {
        var userId = Guid.NewGuid().ToString();
        _mediatorMock.Setup(m => m.Send(It.IsAny<UserActivityQuery>(), default)).ReturnsAsync(new List<UserActivityDto>());

        var result = await _controller.GetByUserIdAsync(userId) as NotFoundObjectResult;

        //Assert.NotNull(result);
        //Assert.Equal(404, result.StatusCode);
        Assert.Equal($"No se encontraron actividades para el usuario con ID {userId}", result.Value);
    }
    #endregion

    #region GetByUserIdAsync_ReturnsInternalServerError_WhenExceptionOccurs
    [Fact]
    public async Task GetByUserIdAsync_ReturnsInternalServerError_WhenExceptionOccurs()
    {
        var userId = Guid.NewGuid().ToString();
        _mediatorMock.Setup(m => m.Send(It.IsAny<UserActivityQuery>(), default)).ThrowsAsync(new Exception("Database failure"));

        var result = await _controller.GetByUserIdAsync(userId) as ObjectResult;

        //Assert.NotNull(result);
        //Assert.Equal(500, result.StatusCode);
        Assert.Equal("Error interno en el servidor.", result.Value);
    }
    #endregion

}
