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

public class UserController_GetAllRoles_Tests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly Mock<IPublishEndpoint> _publishEndpointMock;
    private readonly Mock<ILog> _loggerMock;
    private readonly UsersController _controller;

    public UserController_GetAllRoles_Tests()
    {
        _mediatorMock = new Mock<IMediator>();
        _publishEndpointMock = new Mock<IPublishEndpoint>();
        _controller = new UsersController(_mediatorMock.Object, _publishEndpointMock.Object);
        _loggerMock = new Mock<ILog>();
    }

    #region GetAllRoles_ReturnsOk_WhenRolesExist
    [Fact]
    public async Task GetAllRoles_ReturnsOk_WhenRolesExist()
    {
        var roles = new List<RoleDto>
        {
            new RoleDto { Id = Guid.NewGuid().ToString(), Name = "Admin" },
            new RoleDto { Id = Guid.NewGuid().ToString(), Name = "User" }
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetAllRolesQuery>(), default)).ReturnsAsync(roles);

        var result = await _controller.GetAllRoles() as OkObjectResult;

        //Assert.NotNull(result);
        //Assert.Equal(200, result.StatusCode);
        Assert.Equal(roles, result.Value);
    }
    #endregion

    #region GetAllRoles_ReturnsNotFound_WhenNoRolesExist
    [Fact]
    public async Task GetAllRoles_ReturnsNotFound_WhenNoRolesExist()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetAllRolesQuery>(), default)).ReturnsAsync(new List<RoleDto>());

        var result = await _controller.GetAllRoles() as NotFoundObjectResult;

        //Assert.NotNull(result);
        //Assert.Equal(404, result.StatusCode);
        Assert.Equal("No se encontraron roles.", result.Value);
    }
    #endregion

    #region GetAllRoles_ReturnsInternalServerError_WhenExceptionOccurs
    [Fact]
    public async Task GetAllRoles_ReturnsInternalServerError_WhenExceptionOccurs()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetAllRolesQuery>(), default)).ThrowsAsync(new Exception("Database failure"));

        var result = await _controller.GetAllRoles() as ObjectResult;

        //Assert.NotNull(result);
        //Assert.Equal(500, result.StatusCode);
        Assert.Equal("Error interno en el servidor.", result.Value);
    }
    #endregion

}
