using MongoDB.Bson;
using Moq;

using Usuarios.Infrastructure.Interfaces;
using Usuarios.Infrastructure.Queries;
using Usuarios.Infrastructure.Queries.QueryHandlers;

namespace Usuarios.Tests.Usuarios.Infrastructure.Queries
{
    public class GetAllUsersQueryHandlerTests
    {
        private readonly Mock<IUserReadRepository> _userReadRepositoryMock;
        private readonly GetAllUsersQueryHandler _handler;

        public GetAllUsersQueryHandlerTests()
        {
            _userReadRepositoryMock = new Mock<IUserReadRepository>();
            _handler = new GetAllUsersQueryHandler(_userReadRepositoryMock.Object);
        }

        #region Handle_ReturnsUserList_WhenUsersExist
        [Fact]
        public async Task Handle_ReturnsUserList_WhenUsersExist()
        {
            var query = new GetAllUsersQuery();
            var users = new List<BsonDocument>
            {
                new BsonDocument
                {
                    { "_id", "user1" },
                    { "name", "Emilia" },
                    { "lastName", "Perez" },
                    { "email", "emiliaperez@gmail.com" },
                    { "roleId", "admin" }
                },
                new BsonDocument
                {
                    { "_id", "user2" },
                    { "name", "Juan" },
                    { "lastName", "Lopez" },
                    { "email", "juanlopez@gmail.com" },
                    { "roleId", "user" }
                }
            };

            _userReadRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(users);

            var result = await _handler.Handle(query, CancellationToken.None);

            //Assert.Equal(2, result.Count);
            Assert.Equal("Emilia", result.First().Name);
        }
        #endregion

        #region Handle_ReturnsEmptyList_WhenNoUsersExist
        [Fact]
        public async Task Handle_ReturnsEmptyList_WhenNoUsersExist()
        {
            var query = new GetAllUsersQuery();

            _userReadRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<BsonDocument>()); // Simulación de lista vacía

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.Empty(result);
        }
        #endregion

        #region Handle_ThrowsException_WhenRepositoryFails
        [Fact]
        public async Task Handle_ThrowsException_WhenRepositoryFails()
        {
            var query = new GetAllUsersQuery();

            _userReadRepositoryMock.Setup(r => r.GetAllAsync()).ThrowsAsync(new Exception("Database error"));

            await Assert.ThrowsAsync<Exception>(() => _handler.Handle(query, CancellationToken.None));
        }
        #endregion

    }
}
