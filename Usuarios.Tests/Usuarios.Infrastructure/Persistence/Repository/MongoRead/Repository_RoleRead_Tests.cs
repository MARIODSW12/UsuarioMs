using Microsoft.AspNetCore.Http.HttpResults;
using MongoDB.Bson;
using MongoDB.Driver;
using Moq;
using Usuarios.Domain.Repositories;
using Usuarios.Infrastructure.Configurations;
using Usuarios.Infrastructure.Interfaces;
using Usuarios.Infrastructure.Persistence.Repository;
using Usuarios.Infrastructure.Persistence.Repository.MongoRead;

namespace Usuarios.Tests.Usuarios.Infrastructure.Persistence.Repository.MongoRead
{
    public class RoleReadRepositoryTests
    {
        private readonly Mock<IMongoDatabase> _mongoDatabaseMock;
        private readonly Mock<IMongoCollection<BsonDocument>> _rolesCollectionMock;
        private readonly RoleReadRepository _repository;

        public RoleReadRepositoryTests()
        {
            _mongoDatabaseMock = new Mock<IMongoDatabase>();
            _rolesCollectionMock = new Mock<IMongoCollection<BsonDocument>>();

            _mongoDatabaseMock.Setup(d => d.GetCollection<BsonDocument>("Roles", It.IsAny<MongoCollectionSettings>()))
                              .Returns(_rolesCollectionMock.Object);

            Environment.SetEnvironmentVariable("MONGODB_CNN_READ", "mongodb://localhost:27017");
            Environment.SetEnvironmentVariable("MONGODB_NAME_READ", "test_database");
            var mongoConfigMock = new MongoReadDbConfig();
            mongoConfigMock.db = _mongoDatabaseMock.Object;

            _repository = new RoleReadRepository(mongoConfigMock);
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
            //Assert.Equal(roleId, result.Id.Value);
            Assert.Equal("Admin", result.Name.Value);
        }
        #endregion

        #region GetByIdAsync_ReturnsNull_WhenRoleDoesNotExist
        [Fact]
        public async Task GetByIdAsync_ReturnsNull_WhenRoleDoesNotExist()
        {
            var roleId = "role123";

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

        #region GetByIdAsync_ReturnsRole_WhenRoleHasNoPermissions
        [Fact]
        public async Task GetByIdAsync_ReturnsRole_WhenRoleHasNoPermissions()
        {
            var roleId = "role123";
            var bsonRole = new BsonDocument
            {
                { "_id", roleId },
                { "RoleName", "Guest" },
                { "PermissionIds", new BsonArray() }
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
            Assert.Empty(result.PermissionIds.PermissionIds);
        }
        #endregion

        #region GetByIdAsync_ReturnsRole_WhenRoleHasManyPermissions
        [Fact]
        public async Task GetByIdAsync_ReturnsRole_WhenRoleHasManyPermissions()
        {
            var roleId = "roleAdmin";
            var permissionsList = Enumerable.Range(1, 100).Select(i => $"perm{i}").ToList();  //Simular 100 permisos
            var bsonRole = new BsonDocument
            {
                { "_id", roleId },
                { "RoleName", "SuperAdmin" },
                { "PermissionIds", new BsonArray(permissionsList) }
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
            Assert.Equal(100, result.PermissionIds.PermissionIds.Count);
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


        #region GetAllAsync_ReturnsRolesList_WhenRolesExist
        [Fact]
        public async Task GetAllAsync_ReturnsRolesList_WhenRolesExist()
        {
            var bsonRoles = new List<BsonDocument>
        {
            new BsonDocument { { "_id", "role1" }, { "RoleName", "Admin" }, { "PermissionIds", new BsonArray { "perm1" } } },
            new BsonDocument { { "_id", "role2" }, { "RoleName", "User" }, { "PermissionIds", new BsonArray { "perm2" } } }
        };

            var cursorMock = new Mock<IAsyncCursor<BsonDocument>>();
            cursorMock.SetupSequence(c => c.MoveNextAsync(default))
                      .ReturnsAsync(true)
                      .ReturnsAsync(false);
            cursorMock.Setup(c => c.Current).Returns(bsonRoles);

            _rolesCollectionMock.Setup(c => c.FindAsync(It.IsAny<FilterDefinition<BsonDocument>>(), It.IsAny<FindOptions<BsonDocument, BsonDocument>>(), default))
                                .ReturnsAsync(cursorMock.Object);

            var result = await _repository.GetAllAsync();

            //Assert.Equal(2, result.Count);
            Assert.Equal("Admin", result[0].Name);
        }
        #endregion

        #region GetAllAsync_ReturnsEmptyList_WhenNoRolesExist
        [Fact]
        public async Task GetAllAsync_ReturnsEmptyList_WhenNoRolesExist()
        {
            var cursorMock = new Mock<IAsyncCursor<BsonDocument>>();
            cursorMock.SetupSequence(c => c.MoveNextAsync(default))
                .ReturnsAsync(false);
            cursorMock.Setup(c => c.Current).Returns(new List<BsonDocument>());

            _rolesCollectionMock.Setup(c => c.FindAsync(It.IsAny<FilterDefinition<BsonDocument>>(),
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
            _rolesCollectionMock.Setup(c => c.FindAsync(It.IsAny<FilterDefinition<BsonDocument>>(),
                    It.IsAny<FindOptions<BsonDocument, BsonDocument>>(), default))
                .ThrowsAsync(new Exception("Database error"));

            await Assert.ThrowsAsync<Exception>(() => _repository.GetAllAsync());
        }
        #endregion


        #region AddPermissionsAsync_ReturnsTrue_WhenPermissionsAreAdded
        [Fact]
        public async Task AddPermissionsAsync_ReturnsTrue_WhenPermissionsAreAdded()
        {
            var role = new BsonDocument
        {
            { "_id", "role123" },
            { "PermissionIds", "perm1" }
        };

            var updateResultMock = new UpdateResult.Acknowledged(1, 1, role["_id"]);

            _rolesCollectionMock.Setup(c => c.UpdateOneAsync(It.IsAny<FilterDefinition<BsonDocument>>(), It.IsAny<UpdateDefinition<BsonDocument>>(), null, default))
                                .ReturnsAsync(updateResultMock);

            var result = await _repository.AddPermissionsAsync(role);

            Assert.True(result);
        }
        #endregion

        #region AddPermissionsAsync_ReturnsFalse_WhenNoDocumentIsModified
        [Fact]
        public async Task AddPermissionsAsync_ReturnsFalse_WhenNoDocumentIsModified()
        {
            var role = new BsonDocument
            {
                { "_id", "role123" },
                { "PermissionIds", "perm1" }
            };

            var updateResultMock = new UpdateResult.Acknowledged(0, 0, role["_id"]); //No modifica ningún documento

            _rolesCollectionMock.Setup(c => c.UpdateOneAsync(It.IsAny<FilterDefinition<BsonDocument>>(),
                    It.IsAny<UpdateDefinition<BsonDocument>>(), null, default))
                .ReturnsAsync(updateResultMock);

            var result = await _repository.AddPermissionsAsync(role);

            Assert.False(result);
        }
        #endregion

        #region AddPermissionsAsync_ThrowsException_WhenDatabaseFails
        [Fact]
        public async Task AddPermissionsAsync_ThrowsException_WhenDatabaseFails()
        {
            var role = new BsonDocument
            {
                { "_id", "role123" },
                { "PermissionIds", "perm1" }
            };

            _rolesCollectionMock.Setup(c => c.UpdateOneAsync(It.IsAny<FilterDefinition<BsonDocument>>(),
                    It.IsAny<UpdateDefinition<BsonDocument>>(), null, default))
                .ThrowsAsync(new Exception("Database error"));

            await Assert.ThrowsAsync<Exception>(() => _repository.AddPermissionsAsync(role));
        }
        #endregion


        #region RemovePermissionByIdAsync_RemovesPermission_WhenPermissionExists
        [Fact]
        public async Task RemovePermissionByIdAsync_RemovesPermission_WhenPermissionExists()
        {
            var roleId = "role123";
            var permissionId = "perm1";
            var updateResultMock = new UpdateResult.Acknowledged(1, 1, roleId);

            _rolesCollectionMock.Setup(c => c.UpdateOneAsync(It.IsAny<FilterDefinition<BsonDocument>>(), It.IsAny<UpdateDefinition<BsonDocument>>(), null, default))
                                .ReturnsAsync(updateResultMock);

            await _repository.RemovePermissionByIdAsync(roleId, permissionId);

            _rolesCollectionMock.Verify(c => c.UpdateOneAsync(It.IsAny<FilterDefinition<BsonDocument>>(), It.IsAny<UpdateDefinition<BsonDocument>>(), null, default), Times.Once);
        }
        #endregion

        #region RemovePermissionByIdAsync_DoesNothing_WhenPermissionDoesNotExist
        [Fact]
        public async Task RemovePermissionByIdAsync_DoesNothing_WhenPermissionDoesNotExist()
        {
            var roleId = "role123";
            var permissionId = "permXYZ"; //Un permiso que no está asignado al rol
            var updateResultMock = new UpdateResult.Acknowledged(0, 0, roleId); //No se modifica nada

            _rolesCollectionMock.Setup(c => c.UpdateOneAsync(It.IsAny<FilterDefinition<BsonDocument>>(),
                    It.IsAny<UpdateDefinition<BsonDocument>>(), null, default))
                .ReturnsAsync(updateResultMock);

            await _repository.RemovePermissionByIdAsync(roleId, permissionId);

            _rolesCollectionMock.Verify(c => c.UpdateOneAsync(It.IsAny<FilterDefinition<BsonDocument>>(), It.IsAny<UpdateDefinition<BsonDocument>>(), null, default), Times.Once);
        }
        #endregion

        #region RemovePermissionByIdAsync_ThrowsException_WhenDatabaseFails
        [Fact]
        public async Task RemovePermissionByIdAsync_ThrowsException_WhenDatabaseFails()
        {
            var roleId = "role123";
            var permissionId = "perm1";

            _rolesCollectionMock.Setup(c => c.UpdateOneAsync(It.IsAny<FilterDefinition<BsonDocument>>(),
                    It.IsAny<UpdateDefinition<BsonDocument>>(), null, default))
                .ThrowsAsync(new Exception("Database error"));

            await Assert.ThrowsAsync<Exception>(() => _repository.RemovePermissionByIdAsync(roleId, permissionId));
        }
        #endregion


        #region GetPermissionsByRoleIdAsync_ReturnsPermissions_WhenRoleExists
        [Fact]
        public async Task GetPermissionsByRoleIdAsync_ReturnsPermissions_WhenRoleExists()
        {
            var roleId = "role123";
            var bsonRole = new BsonDocument
            {
                { "_id", roleId },
                { "PermissionIds", new BsonArray { "perm1", "perm2", "perm3" } }
            };

            var cursorMock = new Mock<IAsyncCursor<BsonDocument>>();
            cursorMock.SetupSequence(c => c.MoveNextAsync(default))
                .ReturnsAsync(true)
                .ReturnsAsync(false);
            cursorMock.Setup(c => c.Current).Returns(new List<BsonDocument> { bsonRole });

            _rolesCollectionMock.Setup(c => c.FindAsync(It.IsAny<FilterDefinition<BsonDocument>>(),
                    It.IsAny<FindOptions<BsonDocument, BsonDocument>>(), default))
                .ReturnsAsync(cursorMock.Object);

            var result = await _repository.GetPermissionsByRoleIdAsync(roleId);

            // Assert
            //Assert.NotNull(result);
            Assert.Equal(3, result.Count);
            //Assert.Contains("perm1", result);
            //Assert.Contains("perm2", result);
            //Assert.Contains("perm3", result);
        }
        #endregion

        #region GetPermissionsByRoleIdAsync_ReturnsNull_WhenRoleDoesNotExist
        [Fact]
        public async Task GetPermissionsByRoleIdAsync_ReturnsNull_WhenRoleDoesNotExist()
        {
            var roleId = "nonexistent_role";

            var cursorMock = new Mock<IAsyncCursor<BsonDocument>>();
            cursorMock.SetupSequence(c => c.MoveNextAsync(default))
                .ReturnsAsync(false);
            cursorMock.Setup(c => c.Current).Returns(new List<BsonDocument>());

            _rolesCollectionMock.Setup(c => c.FindAsync(It.IsAny<FilterDefinition<BsonDocument>>(),
                    It.IsAny<FindOptions<BsonDocument, BsonDocument>>(), default))
                .ReturnsAsync(cursorMock.Object);

            var result = await _repository.GetPermissionsByRoleIdAsync(roleId);

            Assert.Null(result);
        }
        #endregion

        #region GetPermissionsByRoleIdAsync_ReturnsEmptyList_WhenRoleHasNoPermissions
        [Fact]
        public async Task GetPermissionsByRoleIdAsync_ReturnsEmptyList_WhenRoleHasNoPermissions()
        {
            var roleId = "role123";
            var bsonRole = new BsonDocument
            {
                { "_id", roleId } //No tiene el campo `PermissionIds`
            };

            var cursorMock = new Mock<IAsyncCursor<BsonDocument>>();
            cursorMock.SetupSequence(c => c.MoveNextAsync(default))
                .ReturnsAsync(true)
                .ReturnsAsync(false);
            cursorMock.Setup(c => c.Current).Returns(new List<BsonDocument> { bsonRole });

            _rolesCollectionMock.Setup(c => c.FindAsync(It.IsAny<FilterDefinition<BsonDocument>>(),
                    It.IsAny<FindOptions<BsonDocument, BsonDocument>>(), default))
                .ReturnsAsync(cursorMock.Object);

            var result = await _repository.GetPermissionsByRoleIdAsync(roleId);

            //Assert.NotNull(result);
            Assert.Empty(result);
        }
        #endregion

        #region GetPermissionsByRoleIdAsync_ThrowsException_WhenDatabaseFails
        [Fact]
        public async Task GetPermissionsByRoleIdAsync_ThrowsException_WhenDatabaseFails()
        {
            var roleId = "role123";

            _rolesCollectionMock.Setup(c => c.FindAsync(It.IsAny<FilterDefinition<BsonDocument>>(),
                    It.IsAny<FindOptions<BsonDocument, BsonDocument>>(), default))
                .ThrowsAsync(new Exception("Database error"));

            await Assert.ThrowsAsync<Exception>(() => _repository.GetPermissionsByRoleIdAsync(roleId));
        }
        #endregion

    }
}
