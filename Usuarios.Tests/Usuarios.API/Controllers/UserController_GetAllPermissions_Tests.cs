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

public class UserController_GetAllPermissions_Tests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly Mock<IPublishEndpoint> _publishEndpointMock;
    private readonly Mock<ILog> _loggerMock;
    private readonly UsersController _controller;

    public UserController_GetAllPermissions_Tests()
    {
        _mediatorMock = new Mock<IMediator>();
        _publishEndpointMock = new Mock<IPublishEndpoint>();
        _controller = new UsersController(_mediatorMock.Object, _publishEndpointMock.Object);
        _loggerMock = new Mock<ILog>();
    }

    #region GetAllPermissions_ReturnsOk_WhenPermissionsExist
    [Fact]
    public async Task GetAllPermissions_ReturnsOk_WhenPermissionsExist()
    {
        var permissions = new List<PermissionDto>
        {
            new PermissionDto { Id = Guid.NewGuid().ToString(), Name = "Read" },
            new PermissionDto { Id = Guid.NewGuid().ToString(), Name = "Write" }
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetAllPermissionsQuery>(), default)).ReturnsAsync(permissions);

        var result = await _controller.GetAllPermissions() as OkObjectResult;

        //Assert.NotNull(result);
        //Assert.Equal(200, result.StatusCode);
        Assert.Equal(permissions, result.Value);
    }
    #endregion

    #region GetAllPermissions_ReturnsNotFound_WhenNoPermissionsExist
    [Fact]
    public async Task GetAllPermissions_ReturnsNotFound_WhenNoPermissionsExist()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetAllPermissionsQuery>(), default)).ReturnsAsync(new List<PermissionDto>());

        var result = await _controller.GetAllPermissions() as NotFoundObjectResult;

        //Assert.NotNull(result);
        //Assert.Equal(404, result.StatusCode);
        Assert.Equal("No se encontraron permisos.", result.Value);
    }
    #endregion

    #region GetAllPermissions_ReturnsInternalServerError_WhenExceptionOccurs
    [Fact]
    public async Task GetAllPermissions_ReturnsInternalServerError_WhenExceptionOccurs()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetAllPermissionsQuery>(), default)).ThrowsAsync(new Exception("Database failure"));

        var result = await _controller.GetAllPermissions() as ObjectResult;

        //Assert.NotNull(result);
        //Assert.Equal(500, result.StatusCode);
        Assert.Equal("Error interno en el servidor.", result.Value);
    }
    #endregion

}
