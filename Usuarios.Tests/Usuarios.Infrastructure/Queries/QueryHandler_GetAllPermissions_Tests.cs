using MongoDB.Bson;
using Moq;

using Usuarios.Infrastructure.Interfaces;
using Usuarios.Infrastructure.Queries;
using Usuarios.Infrastructure.Queries.QueryHandlers;

namespace Usuarios.Tests.Usuarios.Infrastructure.Queries
{
    public class GetAllPermissionsQueryHandlerTests
    {
        private readonly Mock<IPermissionReadRepository> _permissionReadRepositoryMock;
        private readonly GetAllPermissionsQueryHandler _handler;

        public GetAllPermissionsQueryHandlerTests()
        {
            _permissionReadRepositoryMock = new Mock<IPermissionReadRepository>();
            _handler = new GetAllPermissionsQueryHandler(_permissionReadRepositoryMock.Object);
        }

        #region Handle_ReturnsPermissionList_WhenPermissionsExist
        [Fact]
        public async Task Handle_ReturnsPermissionList_WhenPermissionsExist()
        {
            var query = new GetAllPermissionsQuery();
            var permissions = new List<BsonDocument>
            {
                new BsonDocument
                {
                    { "_id", "perm1" },
                    { "PermissionName", "Read" }
                },
                new BsonDocument
                {
                    { "_id", "perm2" },
                    { "PermissionName", "Write" }
                }
            };

            _permissionReadRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(permissions);

            var result = await _handler.Handle(query, CancellationToken.None);

            //Assert.Equal(2, result.Count);
            Assert.Equal("Read", result.First().Name);
        }
        #endregion

        #region Handle_ReturnsEmptyList_WhenNoPermissionsExist
        [Fact]
        public async Task Handle_ReturnsEmptyList_WhenNoPermissionsExist()
        {
            var query = new GetAllPermissionsQuery();

            _permissionReadRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<BsonDocument>()); // Simulación de lista vacía

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.Empty(result);
        }
        #endregion

        #region Handle_ThrowsException_WhenRepositoryFails
        [Fact]
        public async Task Handle_ThrowsException_WhenRepositoryFails()
        {
            var query = new GetAllPermissionsQuery();

            _permissionReadRepositoryMock.Setup(r => r.GetAllAsync()).ThrowsAsync(new Exception("Database error"));

            await Assert.ThrowsAsync<Exception>(() => _handler.Handle(query, CancellationToken.None));
        }
        #endregion

    }
}
