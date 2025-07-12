using Moq;
using MassTransit;

using Usuarios.Application.Commands;
using Usuarios.Application.DTOs;
using Usuarios.Application.Handlers;

using Usuarios.Domain.ValueObjects;
using Usuarios.Domain.Repositories;
using Usuarios.Domain.Aggregates;
using Usuarios.Domain.Events;


namespace Usuarios.Tests.Usuarios.Application.Commands;
public class CommandHandler_UpdateUser_Tests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IPublishEndpoint> _publishEndpointMock;
    private readonly UpdateUserCommandHandler _handler;

    public CommandHandler_UpdateUser_Tests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _publishEndpointMock = new Mock<IPublishEndpoint>();
        _handler = new UpdateUserCommandHandler(_userRepositoryMock.Object, _publishEndpointMock.Object);
    }

    #region UpdateUserCommandHandle_ShouldUpdateUser_WhenDataIsValid
    [Fact]
    public async Task UpdateUserCommandHandle_ShouldUpdateUser_WhenDataIsValid()
    {
        var user = new User(new VOId("550e8400-e29b-41d4-a716-446655440000"),
            new VOName("Test"),
            new VOLastName("User"),
            new VOEmail("test@example.com"),
            new VORoleId("123456"),
            new VOAddress("Av. Caracas"),
            new VOPhone("04123456789"));

        _userRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<string>()))
            .ReturnsAsync(user);

        _userRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<User>()))
            .Returns(Task.CompletedTask);

        var userDto = new UpdateUserDto
        {
            Name = "María",
            LastName = "Pérez",
            Address = "Av. Caracas 123",
            Phone = "04123456789"
        };

        var command = new UpdateUserCommand(userDto, "550e8400-e29b-41d4-a716-446655440000");

        var result = await _handler.Handle(command, default);

        Assert.True(result); //Validar que la actualización fue exitosa
    }
    #endregion

    #region UpdateUserCommandHandle_ShouldThrowException_WhenUserDoesNotExist
    [Fact]
    public async Task UpdateUserCommandHandle_ShouldThrowException_WhenUserDoesNotExist()
    {
        _userRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<string>()))
            .ReturnsAsync((User)null); // 🔹 Simula que el usuario NO existe

        var command = new UpdateUserCommand(new UpdateUserDto
        {
            Name = "María",
            LastName = "Pérez",
            Address = "Av. Caracas 123",
            Phone = "04123456789"
        }, "999999"); // 🔹 ID inexistente

        var exception = await Assert.ThrowsAsync<ArgumentException>(() => _handler.Handle(command, default));

        Assert.Equal("El usuario no existe.", exception.Message);
    }
    #endregion

    #region UpdateUserCommandHandle_ShouldPublish_UserUpdatedEvent
    [Fact]
    public async Task UpdateUserCommandHandle_ShouldPublish_UserUpdatedEvent()
    {
        var user = new User(new VOId("550e8400-e29b-41d4-a716-446655440000"),
            new VOName("Test"),
            new VOLastName("User"),
            new VOEmail("test@example.com"),
            new VORoleId("123456"),
            new VOAddress("Av. Caracas"),
            new VOPhone("04123456789"));

        _userRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<string>()))
            .ReturnsAsync(user);

        _userRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<User>()))
            .Returns(Task.CompletedTask);

        _publishEndpointMock.Setup(p => p.Publish(It.IsAny<UserUpdatedEvent>(), default))
            .Returns(Task.CompletedTask); // 🔹 Simula publicación exitosa

        var command = new UpdateUserCommand(new UpdateUserDto
        {
            Name = "María",
            LastName = "Pérez",
            Address = "Av. Caracas 123",
            Phone = "04123456789"
        }, "550e8400-e29b-41d4-a716-446655440000");

        await _handler.Handle(command, default);

        _publishEndpointMock.Verify(p => p.Publish(It.IsAny<UserUpdatedEvent>(), default), Times.Once); // 🔹 Verifica que el evento se publica correctamente
    }
    #endregion

}
