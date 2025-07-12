using MassTransit;
using Moq;

using Usuarios.Application.Commands;
using Usuarios.Application.DTOs;
using Usuarios.Application.Handlers;

using Usuarios.Domain.Aggregates;
using Usuarios.Domain.Entities;
using Usuarios.Domain.Events;
using Usuarios.Domain.Repositories;
using Usuarios.Domain.ValueObjects;


namespace Usuarios.Tests.Usuarios.Application.Commands;
public class CommandHandler_UpdateUserRole_Tests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IRoleRepository> _roleRepositoryMock;
    private readonly Mock<IPublishEndpoint> _publishEndpointMock;
    private readonly UpdateUserRoleCommandHandler _handler;

    public CommandHandler_UpdateUserRole_Tests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _roleRepositoryMock = new Mock<IRoleRepository>();
        _publishEndpointMock = new Mock<IPublishEndpoint>();

        _handler = new UpdateUserRoleCommandHandler(_userRepositoryMock.Object, _publishEndpointMock.Object, _roleRepositoryMock.Object);
    }

    #region Handle_ReturnsTrue_WhenRoleUpdatedSuccessfully
    [Fact]
    public async Task Handle_ReturnsTrue_WhenRoleUpdatedSuccessfully()
    {
        var command = new UpdateUserRoleCommand("550e8400-e29b-41d4-a716-446655440000", new UpdateUserRoleDto { NewRoleId = "admin" });
        var user = new User(new VOId("550e8400-e29b-41d4-a716-446655440000"), new VOName("Emilia"), new VOLastName("Perez"), new VOEmail("emiliaperez@gmail.com"), new VORoleId("user"));
        var role = new Role(new VORoleId("admin"), new VORoleName("admin"), new VORolePermissions(["1", "2"]));

        _userRepositoryMock.Setup(r => r.GetByIdAsync(command.UserId)).ReturnsAsync(user);
        _roleRepositoryMock.Setup(r => r.GetByIdAsync(command.UserRoleDto.NewRoleId)).ReturnsAsync(role);
        _userRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<User>())).Returns(Task.CompletedTask);
        _publishEndpointMock.Setup(p => p.Publish(It.IsAny<UserRoleUpdatedEvent>(), default)).Returns(Task.CompletedTask);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result);
    }
    #endregion

    #region Handle_ThrowsArgumentException_WhenUserDoesNotExist
    [Fact]
    public async Task Handle_ThrowsArgumentException_WhenUserDoesNotExist()
    {
        var command = new UpdateUserRoleCommand("invalidUser", new UpdateUserRoleDto { NewRoleId = "admin" });

        _userRepositoryMock.Setup(r => r.GetByIdAsync(command.UserId)).ReturnsAsync((User)null);

        await Assert.ThrowsAsync<ArgumentException>(() => _handler.Handle(command, CancellationToken.None));
    }
    #endregion

    #region Handle_ThrowsArgumentException_WhenRoleDoesNotExist
    [Fact]
    public async Task Handle_ThrowsArgumentException_WhenRoleDoesNotExist()
    {
        var command = new UpdateUserRoleCommand("550e8400-e29b-41d4-a716-446655440000", new UpdateUserRoleDto { NewRoleId = "invalidRole" });
        var user = new User(new VOId("550e8400-e29b-41d4-a716-446655440000"), new VOName("Emilia"), new VOLastName("Perez"), new VOEmail("emiliaperez@gmail.com"), new VORoleId("user"));

        _userRepositoryMock.Setup(r => r.GetByIdAsync(command.UserId)).ReturnsAsync(user);
        _roleRepositoryMock.Setup(r => r.GetByIdAsync(command.UserRoleDto.NewRoleId)).ReturnsAsync((Role)null);

        await Assert.ThrowsAsync<ArgumentException>(() => _handler.Handle(command, CancellationToken.None));
    }
    #endregion

    #region Handle_ThrowsException_WhenUserUpdateFails
    [Fact]
    public async Task Handle_ThrowsException_WhenUserUpdateFails()
    {
        var command = new UpdateUserRoleCommand("550e8400-e29b-41d4-a716-446655440000", new UpdateUserRoleDto { NewRoleId = "admin" });
        var user = new User(new VOId("550e8400-e29b-41d4-a716-446655440000"), new VOName("Emilia"), new VOLastName("Perez"), new VOEmail("emiliaperez@gmail.com"), new VORoleId("user"));
        var role = new Role(new VORoleId("admin"), new VORoleName("admin"), new VORolePermissions(["1", "2"]));

        _userRepositoryMock.Setup(r => r.GetByIdAsync(command.UserId)).ReturnsAsync(user);
        _roleRepositoryMock.Setup(r => r.GetByIdAsync(command.UserRoleDto.NewRoleId)).ReturnsAsync(role);
        _userRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<User>())).ThrowsAsync(new Exception("Database failure"));

        await Assert.ThrowsAsync<Exception>(() => _handler.Handle(command, CancellationToken.None));
    }
    #endregion

    #region Handle_ThrowsException_WhenEventPublishingFails
    [Fact]
    public async Task Handle_ThrowsException_WhenEventPublishingFails()
    {
        var command = new UpdateUserRoleCommand("550e8400-e29b-41d4-a716-446655440000", new UpdateUserRoleDto { NewRoleId = "admin" });
        var user = new User(new VOId("550e8400-e29b-41d4-a716-446655440000"), new VOName("Emilia"), new VOLastName("Perez"), new VOEmail("emiliaperez@gmail.com"), new VORoleId("user"));
        var role = new Role(new VORoleId("admin"), new VORoleName("admin"), new VORolePermissions(["1", "2"]));

        _userRepositoryMock.Setup(r => r.GetByIdAsync(command.UserId)).ReturnsAsync(user);
        _roleRepositoryMock.Setup(r => r.GetByIdAsync(command.UserRoleDto.NewRoleId)).ReturnsAsync(role);
        _userRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<User>())).Returns(Task.CompletedTask);
        _publishEndpointMock.Setup(p => p.Publish(It.IsAny<UserRoleUpdatedEvent>(), default)).ThrowsAsync(new Exception("RabbitMQ error"));

        await Assert.ThrowsAsync<Exception>(() => _handler.Handle(command, CancellationToken.None));
    }
    #endregion

}
