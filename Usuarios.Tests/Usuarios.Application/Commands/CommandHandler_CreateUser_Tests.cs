using MediatR;
using Moq;

using Usuarios.Application.Commands;
using Usuarios.Application.DTOs;
using Usuarios.Application.Handlers;

using Usuarios.Domain.Aggregates;
using Usuarios.Domain.Entities;
using Usuarios.Domain.Events;
using Usuarios.Domain.Repositories;
using Usuarios.Domain.ValueObjects;

namespace Usuarios.Tests.Usuarios.Application.Commands
{
    public class CommandHandler_CreateUser_Tests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IRoleRepository> _roleRepositoryMock;
        private readonly Mock<IMediator> _mediatorMock;
        private readonly CreateUserCommandHandler _handler;

        public CommandHandler_CreateUser_Tests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _roleRepositoryMock = new Mock<IRoleRepository>();
            _mediatorMock = new Mock<IMediator>();

            _handler = new CreateUserCommandHandler(_userRepositoryMock.Object, _roleRepositoryMock.Object,
                _mediatorMock.Object);
        }

        #region CreateUserCommandHandle_ShouldCreateUser_WhenDataIsValid
        [Fact]
        public async Task CreateUserCommandHandle_ShouldCreateUser_WhenDataIsValid()
        {
            _roleRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(new Role(new VORoleId("123456"), new VORoleName("Admin"),
                    new VORolePermissions(new List<string> { "READ", "WRITE" })));

            _userRepositoryMock.Setup(u => u.GetByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync((User)null);

            var command = new CreateUserCommand(new CreateUserDto
            {
                Name = "María",
                LastName = "Pérez",
                Email = "maria@example.com",
                RoleId = "123456",
                Address = "Av. Caracas 123",
                Phone = "04123456789"
            });

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.NotEmpty(result); //Validar que el ID retornado es válido
        }
        #endregion

        #region CreateUserCommandHandle_ShouldThrowException_WhenUserAlreadyExists
        [Fact]
        public async Task CreateUserCommandHandle_ShouldThrowException_WhenUserAlreadyExists()
        {
            _roleRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(new Role(new VORoleId("123456"), new VORoleName("Admin"), new VORolePermissions(new List<string> { "READ", "WRITE" })));

            _userRepositoryMock.Setup(u => u.GetByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(new User(new VOId("550e8400-e29b-41d4-a716-446655440000"), new VOName("Test"), new VOLastName("User"), new VOEmail("maria@example.com"), new VORoleId("123456"), new VOAddress("Av. Caracas 123"), new VOPhone("04123456789")));

            var command = new CreateUserCommand(new CreateUserDto
            {
                Name = "María",
                LastName = "Pérez",
                Email = "maria@example.com", //Correo ya registrado
                RoleId = "123456",
                Address = "Av. Caracas 123",
                Phone = "04123456789"
            });

            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _handler.Handle(command, default));

            Assert.Equal("El correo electrónico ya está registrado.", exception.Message);
        }
        #endregion

        #region CreateUserCommandHandle_ShouldThrowException_WhenRoleDoesNotExist
        [Fact]
        public async Task CreateUserCommandHandle_ShouldThrowException_WhenRoleDoesNotExist()
        {
            _roleRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<string>()))
                .ReturnsAsync((Role)null); //Simula que el rol NO existe

            var command = new CreateUserCommand(new CreateUserDto
            {
                Name = "María",
                LastName = "Pérez",
                Email = "maria@example.com",
                RoleId = "999999", //ID de rol inexistente
                Address = "Av. Caracas 123",
                Phone = "04123456789"
            });

            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _handler.Handle(command, default));

            Assert.Equal("El rol especificado no existe.", exception.Message);
        }
        #endregion

        #region CreateUserCommandHandle_ShouldPublish_UserCreatedEvent
        [Fact]
        public async Task CreateUserCommandHandle_ShouldPublish_UserCreatedEvent()
        {
            _roleRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(new Role(new VORoleId("123456"), new VORoleName("Admin"), new VORolePermissions(new List<string> { "READ", "WRITE" })));

            _userRepositoryMock.Setup(u => u.GetByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync((User)null);

            _mediatorMock.Setup(m => m.Publish(It.IsAny<UserCreatedEvent>(), default))
                .Returns(Task.CompletedTask);

            var command = new CreateUserCommand(new CreateUserDto
            {
                Name = "María",
                LastName = "Pérez",
                Email = "maria@example.com",
                RoleId = "123456",
                Address = "Av. Caracas 123",
                Phone = "04123456789"
            });

            await _handler.Handle(command, default);

            //Verificar que `UserCreatedEvent` fue publicado 1 vez
            _mediatorMock.Verify(m => m.Publish(It.IsAny<UserCreatedEvent>(), default), Times.Once);
        }
        #endregion

        #region CreateUserCommandHandle_ShouldCall_AddAsync_WhenUserIsCreated
        [Fact]
        public async Task CreateUserCommandHandle_ShouldCall_AddAsync_WhenUserIsCreated()
        {
            _roleRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(new Role(new VORoleId("123456"), new VORoleName("Admin"), new VORolePermissions(new List<string> { "READ", "WRITE" })));

            _userRepositoryMock.Setup(u => u.GetByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync((User)null);

            var command = new CreateUserCommand(new CreateUserDto
            {
                Name = "María",
                LastName = "Pérez",
                Email = "maria@example.com",
                RoleId = "123456",
                Address = "Av. Caracas 123",
                Phone = "04123456789"
            });

            await _handler.Handle(command, default);

            //Verificar que `AddAsync` se llamó exactamente 1 vez
            _userRepositoryMock.Verify(u => u.AddAsync(It.IsAny<User>()), Times.Once);
        }
        #endregion

        #region CreateUserCommandHandle_ShouldThrowException_WhenUserDtoIsNull
        [Fact]
        public void CreateUserCommandHandle_ShouldThrowException_WhenUserDtoIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new CreateUserCommand(null));
        }
        #endregion

    }
}
