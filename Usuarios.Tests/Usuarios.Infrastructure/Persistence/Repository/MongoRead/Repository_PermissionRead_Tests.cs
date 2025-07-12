using MongoDB.Bson;
using MongoDB.Driver;
using Moq;

using Usuarios.Infrastructure.Configurations;
using Usuarios.Infrastructure.Persistence.Repository.MongoRead;

namespace Usuarios.Tests.Usuarios.Infrastructure.Persistence.Repository.MongoRead
{
    public class PermissionReadRepositoryTests
    {
        private readonly Mock<IMongoDatabase> _mongoDatabaseMock;
        private readonly Mock<IMongoCollection<BsonDocument>> _permissionsCollectionMock;
        private readonly PermissionReadRepository _repository;

        public PermissionReadRepositoryTests()
        {
            _mongoDatabaseMock = new Mock<IMongoDatabase>();
            _permissionsCollectionMock = new Mock<IMongoCollection<BsonDocument>>();
            _mongoDatabaseMock.Setup(d => d.GetCollection<BsonDocument>("Permissions", It.IsAny<MongoCollectionSettings>()))
                .Returns(_permissionsCollectionMock.Object);

            Environment.SetEnvironmentVariable("MONGODB_CNN_READ", "mongodb://localhost:27017");
            Environment.SetEnvironmentVariable("MONGODB_NAME_READ", "test_database");
            var mongoConfig = new MongoReadDbConfig();
            mongoConfig.db = _mongoDatabaseMock.Object;

            _repository = new PermissionReadRepository(mongoConfig);
        }

        #region GetAllAsync_ReturnsPermissionList_WhenPermissionsExist
        [Fact]
        public async Task GetAllAsync_ReturnsPermissionList_WhenPermissionsExist()
        {
            var permissions = new List<BsonDocument>
            {
                new BsonDocument { { "_id", "perm1" }, { "name", "Read" } },
                new BsonDocument { { "_id", "perm2" }, { "name", "Write" } }
            };

            var cursorMock = new Mock<IAsyncCursor<BsonDocument>>();
            cursorMock.SetupSequence(c => c.MoveNextAsync(default))
                .ReturnsAsync(true)
                .ReturnsAsync(false);
            cursorMock.Setup(c => c.Current).Returns(permissions);

            _permissionsCollectionMock.Setup(c => c.FindAsync(It.IsAny<FilterDefinition<BsonDocument>>(),
                    It.IsAny<FindOptions<BsonDocument, BsonDocument>>(), default))
                .ReturnsAsync(cursorMock.Object);

            var result = await _repository.GetAllAsync();

            //Assert.Equal(2, result.Count);
            Assert.Equal("Read", result[0]["name"].AsString);
        }
        #endregion

        #region GetAllAsync_ReturnsEmptyList_WhenNoPermissionsExist
        [Fact]
        public async Task GetAllAsync_ReturnsEmptyList_WhenNoPermissionsExist()
        {
            var cursorMock = new Mock<IAsyncCursor<BsonDocument>>();
            cursorMock.SetupSequence(c => c.MoveNextAsync(default))
                .ReturnsAsync(false);
            cursorMock.Setup(c => c.Current).Returns(new List<BsonDocument>());

            _permissionsCollectionMock.Setup(c => c.FindAsync(It.IsAny<FilterDefinition<BsonDocument>>(),
                    It.IsAny<FindOptions<BsonDocument, BsonDocument>>(), default))
                .ReturnsAsync(cursorMock.Object);

            var result = await _repository.GetAllAsync();

            Assert.Empty(result);
        }
        #endregion

        #region GetAllAsync_ThrowsException_WhenDatabaseFails
        [Fact]
        public async Task GetAllAsync_ThrowsException_WhenDatabaseFails()
        {
            _permissionsCollectionMock.Setup(c => c.FindAsync(It.IsAny<FilterDefinition<BsonDocument>>(),
                    It.IsAny<FindOptions<BsonDocument, BsonDocument>>(), default))
                .ThrowsAsync(new Exception("Database error"));

            await Assert.ThrowsAsync<Exception>(() => _repository.GetAllAsync());
        }
        #endregion

    }
}
