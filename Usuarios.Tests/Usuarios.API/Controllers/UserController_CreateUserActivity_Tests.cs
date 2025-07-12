using log4net;
using MassTransit;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;

using Usuarios.Presentation.Controllers;

using Usuarios.Application.DTOs;
using Usuarios.Application.Events;

namespace Usuarios.Tests.Usuarios.API.Controllers;

public class UserController_CreateUserActivity_Tests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly Mock<IPublishEndpoint> _publishEndpointMock;
    private readonly Mock<ILog> _loggerMock;
    private readonly UsersController _controller;

    public UserController_CreateUserActivity_Tests()
    {
        _mediatorMock = new Mock<IMediator>();
        _publishEndpointMock = new Mock<IPublishEndpoint>();
        _controller = new UsersController(_mediatorMock.Object, _publishEndpointMock.Object);
        _loggerMock = new Mock<ILog>();
    }

    #region PublishUserActivity_ReturnsOk_WhenEventPublishedSuccessfully
    [Fact]
    public async Task PublishUserActivity_ReturnsOk_WhenEventPublishedSuccessfully()
    {
        var createUserActivityDto = new CreateUserActivityDto
        {
            UserId = Guid.NewGuid().ToString(),
            Action = "Login"
        };

        _publishEndpointMock.Setup(p => p.Publish(It.IsAny<UserActivityMadeEvent>(), default)).Returns(Task.CompletedTask);

        var result = await _controller.PublishUserActivity(createUserActivityDto) as OkObjectResult;

        //Assert.NotNull(result);
        //Assert.Equal(200, result.StatusCode);
        Assert.Equal("Evento de actividad publicado correctamente.", result.Value);
    }
    #endregion

    #region PublishUserActivity_ReturnsInternalServerError_WhenExceptionOccurs
    [Fact]
    public async Task PublishUserActivity_ReturnsInternalServerError_WhenExceptionOccurs()
    {
        var createUserActivityDto = new CreateUserActivityDto
        {
            UserId = Guid.NewGuid().ToString(),
            Action = "FailedLogin"
        };

        _publishEndpointMock.Setup(p => p.Publish(It.IsAny<UserActivityMadeEvent>(), default))
            .ThrowsAsync(new Exception("RabbitMQ error"));

        var result = await _controller.PublishUserActivity(createUserActivityDto) as ObjectResult;

        //Assert.NotNull(result);
        //Assert.Equal(500, result.StatusCode);
        Assert.Equal("Error interno en el servidor.", result.Value);
    }
    #endregion

}

