using MongoDB.Bson;
using MongoDB.Driver;
using Moq;

using Usuarios.Domain.Entities;
using Usuarios.Domain.ValueObjects;

using Usuarios.Infrastructure.Configurations;
using Usuarios.Infrastructure.Persistence.Repository.MongoWrite;

namespace Usuarios.Tests.Usuarios.Infrastructure.Persistence.Repository.MongoWrite
{
    public class RoleWriteRepositoryTests
    {
        private readonly Mock<IMongoDatabase> _mongoDatabaseMock;
        private readonly Mock<IMongoCollection<BsonDocument>> _rolesCollectionMock;
        private readonly RoleWriteRepository _repository;

        public RoleWriteRepositoryTests()
        {
            _mongoDatabaseMock = new Mock<IMongoDatabase>();
            _rolesCollectionMock = new Mock<IMongoCollection<BsonDocument>>();

            _mongoDatabaseMock.Setup(d => d.GetCollection<BsonDocument>("Roles", It.IsAny<MongoCollectionSettings>()))
                              .Returns(_rolesCollectionMock.Object);

            Environment.SetEnvironmentVariable("MONGODB_CNN_WRITE", "mongodb://localhost:27017");
            Environment.SetEnvironmentVariable("MONGODB_NAME_WRITE", "test_database");
            var mongoConfigMock = new MongoWriteDbConfig();
            mongoConfigMock.db = _mongoDatabaseMock.Object;

            _repository = new RoleWriteRepository(mongoConfigMock);
        }

        #region GetByIdAsync_ReturnsRole_WhenRoleExists
        [Fact]
        public async Task GetByIdAsync_ReturnsRole_WhenRoleExists()
        {
            var roleId = "role123";
            var bsonRole = new BsonDocument
            {
                { "_id", roleId },
                { "RoleName", "Admin" },
                { "PermissionIds", new BsonArray { "perm1", "perm2" } }
            };

            var cursorMock = new Mock<IAsyncCursor<BsonDocument>>();
            cursorMock.SetupSequence(c => c.MoveNextAsync(default))
                .ReturnsAsync(true)
                .ReturnsAsync(false);
            cursorMock.Setup(c => c.Current).Returns(new List<BsonDocument> { bsonRole });

            _rolesCollectionMock.Setup(c => c.FindAsync(It.IsAny<FilterDefinition<BsonDocument>>(),
                    It.IsAny<FindOptions<BsonDocument, BsonDocument>>(), default))
                .ReturnsAsync(cursorMock.Object);

            var result = await _repository.GetByIdAsync(roleId);

            //Assert.NotNull(result);
            Assert.Equal("Admin", result.Name.Value);
        }
        #endregion

        #region GetByIdAsync_ReturnsNull_WhenRoleDoesNotExist
        [Fact]
        public async Task GetByIdAsync_ReturnsNull_WhenRoleDoesNotExist()
        {
            var roleId = "nonexistent_role";

            var cursorMock = new Mock<IAsyncCursor<BsonDocument>>();
            cursorMock.SetupSequence(c => c.MoveNextAsync(default))
                .ReturnsAsync(false);
            cursorMock.Setup(c => c.Current).Returns(new List<BsonDocument>());

            _rolesCollectionMock.Setup(c => c.FindAsync(It.IsAny<FilterDefinition<BsonDocument>>(),
                    It.IsAny<FindOptions<BsonDocument, BsonDocument>>(), default))
                .ReturnsAsync(cursorMock.Object);

            var result = await _repository.GetByIdAsync(roleId);

            Assert.Null(result);
        }
        #endregion

        #region GetByIdAsync_ThrowsException_WhenDatabaseFails
        [Fact]
        public async Task GetByIdAsync_ThrowsException_WhenDatabaseFails()
        {
            var roleId = "role123";

            _rolesCollectionMock.Setup(c => c.FindAsync(It.IsAny<FilterDefinition<BsonDocument>>(),
                    It.IsAny<FindOptions<BsonDocument, BsonDocument>>(), default))
                .ThrowsAsync(new Exception("Database error"));

            await Assert.ThrowsAsync<Exception>(() => _repository.GetByIdAsync(roleId));
        }
        #endregion


        #region AddPermissionsAsync_ReturnsFalse_WhenRoleDoesNotExist
        [Fact]
        public async Task AddPermissionsAsync_ReturnsFalse_WhenRoleDoesNotExist()
        {
            var roleId = new VORoleId("role123");
            var permissions = new VORolePermissions(new List<string> { "perm1", "perm2" });

            var updateResultMock = new UpdateResult.Acknowledged(0, 0, roleId.Value);

            _rolesCollectionMock.Setup(c => c.UpdateOneAsync(It.IsAny<FilterDefinition<BsonDocument>>(),
                                     It.IsAny<UpdateDefinition<BsonDocument>>(), null, default))
                                .ReturnsAsync(updateResultMock);

            var result = await _repository.AddPermissionsAsync(roleId, permissions);

            Assert.False(result);
        }
        #endregion

        #region AddPermissionsAsync_ReturnsTrue_WhenPermissionsAreAdded
        [Fact]
        public async Task AddPermissionsAsync_ReturnsTrue_WhenPermissionsAreAdded()
        {
            var roleId = new VORoleId("role123");
            var permissions = new VORolePermissions(new List<string> { "perm1", "perm2" });

            var updateResultMock = new UpdateResult.Acknowledged(1, 1, roleId.Value);

            _rolesCollectionMock.Setup(c => c.UpdateOneAsync(It.IsAny<FilterDefinition<BsonDocument>>(),
                    It.IsAny<UpdateDefinition<BsonDocument>>(), null, default))
                .ReturnsAsync(updateResultMock);

            var result = await _repository.AddPermissionsAsync(roleId, permissions);

            Assert.True(result);
        }
        #endregion

        #region AddPermissionsAsync_DoesNotModify_WhenPermissionsAlreadyExist
        [Fact]
        public async Task AddPermissionsAsync_DoesNotModify_WhenPermissionsAlreadyExist()
        {
            var roleId = new VORoleId("role123");
            var permissions = new VORolePermissions(new List<string> { "perm1", "perm2" });

            var bsonRole = new BsonDocument
            {
                { "_id", roleId.Value },
                { "PermissionIds", new BsonArray { "perm1", "perm2" } } // ✅ Los permisos ya existen
            };

            var cursorMock = new Mock<IAsyncCursor<BsonDocument>>();
            cursorMock.SetupSequence(c => c.MoveNextAsync(default))
                .ReturnsAsync(true)
                .ReturnsAsync(false);
            cursorMock.Setup(c => c.Current).Returns(new List<BsonDocument> { bsonRole });

            _rolesCollectionMock.Setup(c => c.FindAsync(It.IsAny<FilterDefinition<BsonDocument>>(),
                    It.IsAny<FindOptions<BsonDocument, BsonDocument>>(), default))
                .ReturnsAsync(cursorMock.Object);

            var updateResultMock = new UpdateResult.Acknowledged(0, 0, roleId.Value); // ✅ No se modifica nada

            _rolesCollectionMock.Setup(c => c.UpdateOneAsync(It.IsAny<FilterDefinition<BsonDocument>>(),
                    It.IsAny<UpdateDefinition<BsonDocument>>(), null, default))
                .ReturnsAsync(updateResultMock);

            var result = await _repository.AddPermissionsAsync(roleId, permissions);

            Assert.False(result);
        }
        #endregion

        #region AddPermissionsAsync_ThrowsException_WhenDatabaseFails
        [Fact]
        public async Task AddPermissionsAsync_ThrowsException_WhenDatabaseFails()
        {
            var roleId = new VORoleId("role123");
            var permissions = new VORolePermissions(new List<string> { "perm1", "perm2" });

            _rolesCollectionMock.Setup(c => c.UpdateOneAsync(It.IsAny<FilterDefinition<BsonDocument>>(),
                    It.IsAny<UpdateDefinition<BsonDocument>>(), null, default))
                .ThrowsAsync(new Exception("Database error"));

            await Assert.ThrowsAsync<Exception>(() => _repository.AddPermissionsAsync(roleId, permissions));
        }
        #endregion


        #region UpdateAsync_ReturnsFalse_WhenRoleDoesNotExist
        [Fact]
        public async Task UpdateAsync_ReturnsFalse_WhenRoleDoesNotExist()
        {
            var role = new Role(new VORoleId("role123"), new VORoleName("Admin"), new VORolePermissions(new List<string> { "perm1", "perm2" }));

            var updateResultMock = new UpdateResult.Acknowledged(0, 0, role.Id.Value);

            _rolesCollectionMock.Setup(c => c.UpdateOneAsync(It.IsAny<FilterDefinition<BsonDocument>>(),
                                     It.IsAny<UpdateDefinition<BsonDocument>>(), null, default))
                                .ReturnsAsync(updateResultMock);

            var result = await _repository.UpdateAsync(role);

            Assert.False(result);
        }
        #endregion

        #region UpdateAsync_ReturnsTrue_WhenRoleIsUpdatedSuccessfully
        [Fact]
        public async Task UpdateAsync_ReturnsTrue_WhenRoleIsUpdatedSuccessfully()
        {
            var role = new Role(new VORoleId("role123"), new VORoleName("Admin"), new VORolePermissions(new List<string> { "perm1", "perm2" }));

            var updateResultMock = new UpdateResult.Acknowledged(1, 1, role.Id.Value); // ✅ Se actualiza correctamente

            _rolesCollectionMock.Setup(c => c.UpdateOneAsync(It.IsAny<FilterDefinition<BsonDocument>>(),
                    It.IsAny<UpdateDefinition<BsonDocument>>(), null, default))
                .ReturnsAsync(updateResultMock);

            var result = await _repository.UpdateAsync(role);

            Assert.True(result);
        }
        #endregion

        #region UpdateAsync_ThrowsException_WhenDatabaseFails
        [Fact]
        public async Task UpdateAsync_ThrowsException_WhenDatabaseFails()
        {
            var role = new Role(new VORoleId("role123"), new VORoleName("Admin"), new VORolePermissions(new List<string> { "perm1", "perm2" }));

            _rolesCollectionMock.Setup(c => c.UpdateOneAsync(It.IsAny<FilterDefinition<BsonDocument>>(),
                    It.IsAny<UpdateDefinition<BsonDocument>>(), null, default))
                .ThrowsAsync(new Exception("Database error"));

            await Assert.ThrowsAsync<Exception>(() => _repository.UpdateAsync(role));
        }
        #endregion

    }
}
