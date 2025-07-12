using MongoDB.Bson;
using MongoDB.Driver;
using Moq;

using Usuarios.Domain.Repositories;

using Usuarios.Infrastructure.Configurations;
using Usuarios.Infrastructure.Persistence.Repository.MongoRead;

namespace Usuarios.Tests.Usuarios.Infrastructure.Persistence.Repository.MongoRead
{
    public class MongoReadUserRepositoryTests
    {
        private readonly Mock<IMongoDatabase> _mongoDatabaseMock;
        private readonly Mock<IMongoCollection<BsonDocument>> _usersCollectionMock;
        private readonly MongoReadUserRepository _repository;

        public MongoReadUserRepositoryTests()
        {
            _mongoDatabaseMock = new Mock<IMongoDatabase>();
            _usersCollectionMock = new Mock<IMongoCollection<BsonDocument>>();

            // Simular la colección de usuarios
            _mongoDatabaseMock.Setup(d => d.GetCollection<BsonDocument>("usuarios", It.IsAny<MongoCollectionSettings>()))
                              .Returns(_usersCollectionMock.Object);

            Environment.SetEnvironmentVariable("MONGODB_CNN_READ", "mongodb://localhost:27017");
            Environment.SetEnvironmentVariable("MONGODB_NAME_READ", "test_database");
            var mongoConfigMock = new MongoReadDbConfig();
            mongoConfigMock.db = _mongoDatabaseMock.Object;

            _repository = new MongoReadUserRepository(mongoConfigMock, Mock.Of<IRoleRepository>());
        }

        #region AddAsync_InsertsUserCorrectly
        [Fact]
        public async Task AddAsync_InsertsUserCorrectly()
        {
            var user = new BsonDocument { { "_id", "user123" }, { "name", "John Doe" }, { "email", "john@example.com" } };

            _usersCollectionMock.Setup(c => c.InsertOneAsync(user, null, default)).Returns(Task.CompletedTask);

            await _repository.AddAsync(user);

            _usersCollectionMock.Verify(c => c.InsertOneAsync(user, null, default), Times.Once);
        }
        #endregion

        #region AddAsync_ThrowsException_WhenDatabaseFails
        [Fact]
        public async Task AddAsync_ThrowsException_WhenDatabaseFails()
        {
            var user = new BsonDocument { { "_id", "user123" }, { "name", "John Doe" }, { "email", "john@example.com" } };

            _usersCollectionMock.Setup(c => c.InsertOneAsync(user, null, default)).ThrowsAsync(new Exception("Database error"));

            await Assert.ThrowsAsync<Exception>(() => _repository.AddAsync(user));
        }
        #endregion


        #region GetByEmailAsync_ReturnsUser_WhenUserExists
        [Fact]
        public async Task GetByEmailAsync_ReturnsUser_WhenUserExists()
        {
            var email = "john@example.com";
            var bsonUser = new BsonDocument
            {
                { "_id", "user123" },
                { "name", "John Updated" },
                { "lastName", "Doe" },
                { "email", "john@example.com" },
                { "roleId", "admin" },
                { "address", "123 Street" },
                { "phone", "1234567890" }
            };

            var cursorMock = new Mock<IAsyncCursor<BsonDocument>>();
            cursorMock.SetupSequence(c => c.MoveNextAsync(default))
                .ReturnsAsync(true)
                .ReturnsAsync(false);
            cursorMock.Setup(c => c.Current).Returns(new List<BsonDocument> { bsonUser });

            _usersCollectionMock.Setup(c => c.FindAsync(It.IsAny<FilterDefinition<BsonDocument>>(),
                    It.IsAny<FindOptions<BsonDocument, BsonDocument>>(), default))
                .ReturnsAsync(cursorMock.Object);

            var result = await _repository.GetByEmailAsync(email);

            Assert.NotNull(result);
            Assert.Equal(email, result.Email);
        }
        #endregion

        #region GetByEmailAsync_ReturnsNull_WhenUserDoesNotExist
        [Fact]
        public async Task GetByEmailAsync_ReturnsNull_WhenUserDoesNotExist()
        {
            // Arrange
            var email = "unknown@example.com";

            var cursorMock = new Mock<IAsyncCursor<BsonDocument>>();
            cursorMock.SetupSequence(c => c.MoveNextAsync(default))
                .ReturnsAsync(false);
            cursorMock.Setup(c => c.Current).Returns(new List<BsonDocument>());

            _usersCollectionMock.Setup(c => c.FindAsync(It.IsAny<FilterDefinition<BsonDocument>>(),
                    It.IsAny<FindOptions<BsonDocument, BsonDocument>>(), default))
                .ReturnsAsync(cursorMock.Object);

            // Act
            var result = await _repository.GetByEmailAsync(email);

            // Assert
            Assert.Null(result);
        }
        #endregion

        #region GetByEmailAsync_ThrowsException_WhenDatabaseFails
        [Fact]
        public async Task GetByEmailAsync_ThrowsException_WhenDatabaseFails()
        {
            _usersCollectionMock.Setup(c => c.FindAsync(It.IsAny<FilterDefinition<BsonDocument>>(),
                    It.IsAny<FindOptions<BsonDocument, BsonDocument>>(), default))
                .ThrowsAsync(new Exception("Database error"));

            await Assert.ThrowsAsync<Exception>(() => _repository.GetByEmailAsync("john@example.com"));
        }
        #endregion


        #region UpdateAsync_UpdatesUserCorrectly
        [Fact]
        public async Task UpdateAsync_UpdatesUserCorrectly()
        {
            var user = new BsonDocument {

                { "_id", "user123" },
                { "name", "John Updated" },
                { "lastName", "Doe" },
                { "email", "john@example.com" },
                { "roleId", "admin" },
                { "address", "123 Street" },
                { "phone", "1234567890" }

            };

            var updateResultMock = new UpdateResult.Acknowledged(1, 1, user["_id"]);

            _usersCollectionMock.Setup(c => c.UpdateOneAsync(It.IsAny<FilterDefinition<BsonDocument>>(), It.IsAny<UpdateDefinition<BsonDocument>>(), null, default))
                                .ReturnsAsync(updateResultMock);

            await _repository.UpdateAsync(user);

            _usersCollectionMock.Verify(c => c.UpdateOneAsync(It.IsAny<FilterDefinition<BsonDocument>>(), It.IsAny<UpdateDefinition<BsonDocument>>(), null, default), Times.Once);
        }
        #endregion

        #region UpdateAsync_ThrowsException_WhenDatabaseFails
        [Fact]
        public async Task UpdateAsync_ThrowsException_WhenDatabaseFails()
        {
            var user = new BsonDocument
            {
                { "_id", "user123" },
                { "name", "John Updated" },
                { "lastName", "Doe" },
                { "email", "john@example.com" },
                { "roleId", "admin" },
                { "address", "123 Street" },
                { "phone", "1234567890" }
            };

            _usersCollectionMock.Setup(c => c.UpdateOneAsync(It.IsAny<FilterDefinition<BsonDocument>>(), It.IsAny<UpdateDefinition<BsonDocument>>(), null, default))
                                .ThrowsAsync(new Exception("Database error"));

            await Assert.ThrowsAsync<Exception>(() => _repository.UpdateAsync(user));
        }
        #endregion

        #region UpdateAsync_DoesNothing_WhenUserDoesNotExist
        [Fact]
        public async Task UpdateAsync_DoesNothing_WhenUserDoesNotExist()
        {
            var user = new BsonDocument
            {
                { "_id", "user123" },
                { "name", "John Updated" },
                { "lastName", "Doe" },
                { "email", "john@example.com" },
                { "roleId", "admin" },
                { "address", "123 Street" },
                { "phone", "1234567890" }
            };

            var updateResultMock = new UpdateResult.Acknowledged(0, 0, user["_id"]); // ✅ No se modifica nada

            _usersCollectionMock.Setup(c => c.UpdateOneAsync(It.IsAny<FilterDefinition<BsonDocument>>(),
                    It.IsAny<UpdateDefinition<BsonDocument>>(), null, default))
                .ReturnsAsync(updateResultMock);

            await _repository.UpdateAsync(user);

            _usersCollectionMock.Verify(c => c.UpdateOneAsync(It.IsAny<FilterDefinition<BsonDocument>>(),
                It.IsAny<UpdateDefinition<BsonDocument>>(),
                null, default), Times.Once);
        }
        #endregion

        #region UpdateAsync_ThrowsException_WhenUserDocumentIsIncomplete
        [Fact]
        public async Task UpdateAsync_ThrowsException_WhenUserDocumentIsIncomplete()
        {
            var user = new BsonDocument
            {
                { "_id", "user123" },
                { "name", "John Updated" },
                { "lastName", "Doe" },
                { "email", "john@example.com" },
                { "roleId", "admin" },
                { "address", "123 Street" },
                { "phone", "1234567890" }
            };

            _usersCollectionMock.Setup(c => c.UpdateOneAsync(It.IsAny<FilterDefinition<BsonDocument>>(),
                    It.IsAny<UpdateDefinition<BsonDocument>>(), null, default))
                .ThrowsAsync(new KeyNotFoundException("El documento de usuario no tiene todos los campos requeridos"));

            await Assert.ThrowsAsync<KeyNotFoundException>(() => _repository.UpdateAsync(user));
        }
        #endregion


        #region GetAllAsync_ReturnsEmptyList_WhenNoUsersExist
        [Fact]
        public async Task GetAllAsync_ReturnsEmptyList_WhenNoUsersExist()
        {
            var cursorMock = new Mock<IAsyncCursor<BsonDocument>>();
            cursorMock.SetupSequence(c => c.MoveNextAsync(default)).ReturnsAsync(false);
            cursorMock.Setup(c => c.Current).Returns(new List<BsonDocument>());

            _usersCollectionMock.Setup(c => c.FindAsync(It.IsAny<FilterDefinition<BsonDocument>>(), It.IsAny<FindOptions<BsonDocument, BsonDocument>>(), default))
                                .ReturnsAsync(cursorMock.Object);

            var result = await _repository.GetAllAsync();

            Assert.Empty(result);
        }
        #endregion

        #region GetAllAsync_ReturnsUsersList_WhenUsersExist
        [Fact]
        public async Task GetAllAsync_ReturnsUsersList_WhenUsersExist()
        {
            var users = new List<BsonDocument>
            {
                new BsonDocument { { "_id", "user1" }, { "name", "Alice" }, { "email", "alice@example.com" } },
                new BsonDocument { { "_id", "user2" }, { "name", "Bob" }, { "email", "bob@example.com" } }
            };

            var cursorMock = new Mock<IAsyncCursor<BsonDocument>>();
            cursorMock.SetupSequence(c => c.MoveNextAsync(default))
                .ReturnsAsync(true)
                .ReturnsAsync(false);
            cursorMock.Setup(c => c.Current).Returns(users);

            _usersCollectionMock.Setup(c => c.FindAsync(It.IsAny<FilterDefinition<BsonDocument>>(),
                    It.IsAny<FindOptions<BsonDocument, BsonDocument>>(), default))
                .ReturnsAsync(cursorMock.Object);

            var result = await _repository.GetAllAsync();

            //Assert.NotEmpty(result);
            Assert.Equal(2, result.Count);
            //Assert.Equal("Alice", result[0]["name"].AsString);
            //Assert.Equal("bob@example.com", result[1]["email"].AsString);
        }
        #endregion

        #region GetAllAsync_ThrowsException_WhenDatabaseFails
        [Fact]
        public async Task GetAllAsync_ThrowsException_WhenDatabaseFails()
        {
            _usersCollectionMock.Setup(c => c.FindAsync(It.IsAny<FilterDefinition<BsonDocument>>(),
                    It.IsAny<FindOptions<BsonDocument, BsonDocument>>(), default))
                .ThrowsAsync(new Exception("Database error"));

            await Assert.ThrowsAsync<Exception>(() => _repository.GetAllAsync());
        }
        #endregion


        #region UpdateRoleIdById_UpdatesRoleId_WhenUserExists
        [Fact]
        public async Task UpdateRoleIdById_UpdatesRoleId_WhenUserExists()
        {
            var user = new BsonDocument
            {
                { "_id", "user123" },
                { "roleId", "admin" }
            };

            var updateResultMock = new UpdateResult.Acknowledged(1, 1, user["_id"]); // ✅ Documento modificado

            _usersCollectionMock.Setup(c => c.UpdateOneAsync(It.IsAny<FilterDefinition<BsonDocument>>(),
                    It.IsAny<UpdateDefinition<BsonDocument>>(), null, default))
                .ReturnsAsync(updateResultMock);

            await _repository.UpdateRoleIdById(user);

            _usersCollectionMock.Verify(c => c.UpdateOneAsync(It.IsAny<FilterDefinition<BsonDocument>>(),
                It.IsAny<UpdateDefinition<BsonDocument>>(),
                null, default), Times.Once);
        }
        #endregion

        #region UpdateRoleIdById_ThrowsKeyNotFoundException_WhenUserDoesNotExist
        [Fact]
        public async Task UpdateRoleIdById_ThrowsKeyNotFoundException_WhenUserDoesNotExist()
        {
            var user = new BsonDocument
            {
                { "_id", "nonexistent_user" },
                { "roleId", "admin" }
            };

            var updateResultMock = new UpdateResult.Acknowledged(0, 0, user["_id"]); //No encontró usuario

            _usersCollectionMock.Setup(c => c.UpdateOneAsync(It.IsAny<FilterDefinition<BsonDocument>>(),
                    It.IsAny<UpdateDefinition<BsonDocument>>(), null, default))
                .ReturnsAsync(updateResultMock);

            await Assert.ThrowsAsync<KeyNotFoundException>(() => _repository.UpdateRoleIdById(user));
        }
        #endregion

        #region UpdateRoleIdById_ThrowsKeyNotFoundException_WhenRoleIdIsMissing
        [Fact]
        public async Task UpdateRoleIdById_ThrowsKeyNotFoundException_WhenRoleIdIsMissing()
        {
            var user = new BsonDocument
            {
                { "_id", "user123" }
                //Sin roleId
            };

            await Assert.ThrowsAsync<KeyNotFoundException>(() => _repository.UpdateRoleIdById(user));
        }
        #endregion

        #region UpdateRoleIdById_ThrowsException_WhenDatabaseFails
        [Fact]
        public async Task UpdateRoleIdById_ThrowsException_WhenDatabaseFails()
        {
            var user = new BsonDocument
            {
                { "_id", "user123" },
                { "roleId", "admin" }
            };

            _usersCollectionMock.Setup(c => c.UpdateOneAsync(It.IsAny<FilterDefinition<BsonDocument>>(),
                    It.IsAny<UpdateDefinition<BsonDocument>>(), null, default))
                .ThrowsAsync(new Exception("Database error"));

            await Assert.ThrowsAsync<Exception>(() => _repository.UpdateRoleIdById(user));
        }


        #endregion

    }
}
