using Moq;

using Usuarios.Infrastructure.Interfaces;
using Usuarios.Infrastructure.Persistence.Repository.MongoRead.Documents;
using Usuarios.Infrastructure.Queries;
using Usuarios.Infrastructure.Queries.QueryHandlers;

namespace Usuarios.Tests.Usuarios.Infrastructure.Queries
{
    public class GetUserByEmailQueryHandlerTests
    {
        private readonly Mock<IUserReadRepository> _userReadRepositoryMock;
        private readonly GetUserByEmailQueryHandler _handler;

        public GetUserByEmailQueryHandlerTests()
        {
            _userReadRepositoryMock = new Mock<IUserReadRepository>();
            _handler = new GetUserByEmailQueryHandler(_userReadRepositoryMock.Object);
        }

        #region Handle_ReturnsUserByEmailDto_WhenUserExists
        [Fact]
        public async Task Handle_ReturnsUserByEmailDto_WhenUserExists()
        {
            var query = new GetUserByEmailQuery("emiliaperez@gmail.com");
            var user = new UserMongoRead
            {
                Id = "550e8400-e29b-41d4-a716-446655440000",
                Name = "Emilia",
                LastName = "Perez",
                Email = "emiliaperez@gmail.com",
                roleId = "user",
                Address = "Av. Principal 123",
                Phone = "04141234567"
            };

            _userReadRepositoryMock.Setup(r => r.GetByEmailAsync(query.Email)).ReturnsAsync(user);

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.Equal("emiliaperez@gmail.com", result.Email);
        }
        #endregion

        #region Handle_ThrowsKeyNotFoundException_WhenUserDoesNotExist
        [Fact]
        public async Task Handle_ThrowsKeyNotFoundException_WhenUserDoesNotExist()
        {
            var query = new GetUserByEmailQuery("nonexistent@example.com");

            _userReadRepositoryMock.Setup(r => r.GetByEmailAsync(It.IsAny<string>())).ReturnsAsync(
                (UserMongoRead)null // Simulación de usuario inexistente
            );

            await Assert.ThrowsAsync<KeyNotFoundException>(() => _handler.Handle(query, CancellationToken.None));
        }
        #endregion

        #region Handle_ThrowsException_WhenRepositoryFails
        [Fact]
        public async Task Handle_ThrowsException_WhenRepositoryFails()
        {
            var query = new GetUserByEmailQuery("error@example.com");

            _userReadRepositoryMock.Setup(r => r.GetByEmailAsync(query.Email)).ThrowsAsync(new Exception("Database error"));

            await Assert.ThrowsAsync<Exception>(() => _handler.Handle(query, CancellationToken.None));
        }
        #endregion

    }
}
