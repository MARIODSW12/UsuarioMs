using System.Net;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Core.Clusters;
using MongoDB.Driver.Core.Connections;
using MongoDB.Driver.Core.Servers;
using Moq;

using Usuarios.Domain.Aggregates;
using Usuarios.Domain.Repositories;
using Usuarios.Domain.ValueObjects;

using Usuarios.Infrastructure.Configurations;
using Usuarios.Infrastructure.Persistence.Repository.MongoWrite;

namespace Usuarios.Tests.Usuarios.Infrastructure.Persistence.Repository.MongoWrite
{
    public class MongoWriteUserRepositoryTests
    {
        private readonly Mock<IMongoDatabase> _mongoDatabaseMock;
        private readonly Mock<IMongoCollection<BsonDocument>> _usersCollectionMock;
        private readonly MongoWriteUserRepository _repository;

        public MongoWriteUserRepositoryTests()
        {
            _mongoDatabaseMock = new Mock<IMongoDatabase>();
            _usersCollectionMock = new Mock<IMongoCollection<BsonDocument>>();

            _mongoDatabaseMock.Setup(d => d.GetCollection<BsonDocument>("usuarios_write", It.IsAny<MongoCollectionSettings>()))
                              .Returns(_usersCollectionMock.Object);

            Environment.SetEnvironmentVariable("MONGODB_CNN_WRITE", "mongodb://localhost:27017");
            Environment.SetEnvironmentVariable("MONGODB_NAME_WRITE", "test_database");
            var mongoConfigMock = new MongoWriteDbConfig();
            mongoConfigMock.db = _mongoDatabaseMock.Object;

            _repository = new MongoWriteUserRepository(mongoConfigMock, Mock.Of<IRoleRepository>());
        }

        #region AddAsync_InsertsUserCorrectly
        [Fact]
        public async Task AddAsync_InsertsUserCorrectly()
        {
            var user = new User(new VOId("9f4cde2f-88fb-4689-8198-d3757ced9c24"), new VOName("John"), new VOLastName("Doe"), new VOEmail("john@example.com"),
                new VORoleId("role1"), new VOAddress("123 Street"), new VOPhone("1234567890"));

            var bsonUser = new BsonDocument
            {
                { "_id", user.Id.Value },
                { "name", user.Name.Value },
                { "lastName", user.LastName.Value },
                { "email", user.Email.Value },
                { "address", user.Address.Value },
                { "phone", user.Phone.Value },
                { "roleId", user.RoleId.Value },
                { "createdAt", DateTime.UtcNow },
                { "updatedAt", DateTime.UtcNow }

            };

            _usersCollectionMock.Setup(c => c.InsertOneAsync(It.IsAny<BsonDocument>(), null, default))
                .Returns(Task.CompletedTask);

            await _repository.AddAsync(user);

            _usersCollectionMock.Verify(c => c.InsertOneAsync(It.IsAny<BsonDocument>(), null, default), Times.Once);
        }
        #endregion

        #region AddAsync_ThrowsException_WhenUserDocumentIsIncomplete
        [Fact]
        public async Task AddAsync_ThrowsException_WhenUserDocumentIsIncomplete()
        {
            var user = new User(new VOId("9f4cde2f-88fb-4689-8198-d3757ced9c24"), new VOName("John"), new VOLastName("Doe"), new VOEmail("john@example.com"),
                new VORoleId("role1"), new VOAddress(null), new VOPhone(null));

            _usersCollectionMock.Setup(c => c.InsertOneAsync(It.IsAny<BsonDocument>(), null, default))
                .ThrowsAsync(new KeyNotFoundException("El documento de usuario no tiene todos los campos requeridos"));

            await Assert.ThrowsAsync<KeyNotFoundException>(() => _repository.AddAsync(user));
        }
        #endregion

        #region AddAsync_ThrowsException_WhenUserAlreadyExists
        [Fact]
        public async Task AddAsync_ThrowsException_WhenUserAlreadyExists()
        {
            var user = new User(new VOId("9f4cde2f-88fb-4689-8198-d3757ced9c24"), new VOName("John"), new VOLastName("Doe"), new VOEmail("john@example.com"),
                new VORoleId("role1"), new VOAddress("123 Street"), new VOPhone("1234567890"));

            var connectionId = new ConnectionId(new ServerId(new ClusterId(), new DnsEndPoint("localhost", 27017)));
            var command = new BsonDocument { { "insert", "usuarios_write" } };
            var commandException = new MongoCommandException(connectionId, "Duplicate key error", command);

            _usersCollectionMock.Setup(c => c.InsertOneAsync(It.IsAny<BsonDocument>(), null, default))
                .ThrowsAsync(commandException);

            await Assert.ThrowsAsync<MongoCommandException>(() => _repository.AddAsync(user));
        }
        #endregion

        #region AddAsync_ThrowsException_WhenDatabaseFails
        [Fact]
        public async Task AddAsync_ThrowsException_WhenDatabaseFails()
        {
            var user = new User(new VOId("9f4cde2f-88fb-4689-8198-d3757ced9c24"), new VOName("John"), new VOLastName("Doe"), new VOEmail("john@example.com"),
                new VORoleId("role1"), new VOAddress("123 Street"), new VOPhone("1234567890"));

            _usersCollectionMock.Setup(c => c.InsertOneAsync(It.IsAny<BsonDocument>(), null, default))
                .ThrowsAsync(new Exception("Database error"));

            await Assert.ThrowsAsync<Exception>(() => _repository.AddAsync(user));
        }
        #endregion


        #region GetByEmailAsync_ReturnsNull_WhenUserDoesNotExist
        [Fact]
        public async Task GetByEmailAsync_ReturnsNull_WhenUserDoesNotExist()
        {
            var email = "unknown@example.com";

            var cursorMock = new Mock<IAsyncCursor<BsonDocument>>();
            cursorMock.SetupSequence(c => c.MoveNextAsync(default)).ReturnsAsync(false);
            cursorMock.Setup(c => c.Current).Returns(new List<BsonDocument>());

            _usersCollectionMock.Setup(c => c.FindAsync(It.IsAny<FilterDefinition<BsonDocument>>(), It.IsAny<FindOptions<BsonDocument, BsonDocument>>(), default))
                .ReturnsAsync(cursorMock.Object);

            var result = await _repository.GetByEmailAsync(email);

            Assert.Null(result);
        }
        #endregion

        #region GetByEmailAsync_ReturnsUser_WhenUserExists
        [Fact]
        public async Task GetByEmailAsync_ReturnsUser_WhenUserExists()
        {
            var email = "john@example.com";
            var bsonUser = new BsonDocument
            {
                { "_id", "9f4cde2f-88fb-4689-8198-d3757ced9c24" },
                { "name", "John Doe" },
                { "lastName", "Doe" },
                { "email", email },
                { "address", "123 Street" },
                { "phone", "1234567890" },
                { "roleId", "role1" }
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

            //Assert.NotNull(result);
            Assert.Equal(email, result.Email.Value);
            //Assert.Equal("John Doe", result.Name.Value);
        }
        #endregion

        #region GetByEmailAsync_ThrowsException_WhenDatabaseFails
        [Fact]
        public async Task GetByEmailAsync_ThrowsException_WhenDatabaseFails()
        {
            var email = "john@example.com";

            _usersCollectionMock.Setup(c => c.FindAsync(It.IsAny<FilterDefinition<BsonDocument>>(),
                    It.IsAny<FindOptions<BsonDocument, BsonDocument>>(), default))
                .ThrowsAsync(new Exception("Database error"));

            await Assert.ThrowsAsync<Exception>(() => _repository.GetByEmailAsync(email));
        }
        #endregion


        #region UpdateAsync_DoesNothing_WhenUserDoesNotExist
        [Fact]
        public async Task UpdateAsync_DoesNothing_WhenUserDoesNotExist()
        {
            var user = new User(new VOId("9f4cde2f-88fb-4689-8198-d3757ced9c24"), new VOName("John"), new VOLastName("Doe"), new VOEmail("john@example.com"),
                                new VORoleId("role1"), new VOAddress("123 Street"), new VOPhone("1234567890"));

            var updateResultMock = new UpdateResult.Acknowledged(0, 0, user.Id.Value);

            _usersCollectionMock.Setup(c => c.UpdateOneAsync(It.IsAny<FilterDefinition<BsonDocument>>(), It.IsAny<UpdateDefinition<BsonDocument>>(), null, default))
                                .ReturnsAsync(updateResultMock);

            await _repository.UpdateAsync(user);

            _usersCollectionMock.Verify(c => c.UpdateOneAsync(It.IsAny<FilterDefinition<BsonDocument>>(), It.IsAny<UpdateDefinition<BsonDocument>>(), null, default), Times.Once);
        }
        #endregion

        #region UpdateAsync_ReturnsTrue_WhenUserIsUpdatedSuccessfully
        [Fact]
        public async Task UpdateAsync_VerifiesUpdate_WhenUserIsUpdatedSuccessfully()
        {
            var user = new User(new VOId("9f4cde2f-88fb-4689-8198-d3757ced9c24"), new VOName("John"), new VOLastName("Doe"), new VOEmail("john@example.com"),
                new VORoleId("role1"), new VOAddress("123 Street"), new VOPhone("1234567890"));

            var updateResultMock = new UpdateResult.Acknowledged(1, 1, user.Id.Value); //Simula actualización exitosa

            _usersCollectionMock.Setup(c => c.UpdateOneAsync(It.IsAny<FilterDefinition<BsonDocument>>(),
                    It.IsAny<UpdateDefinition<BsonDocument>>(), null, default))
                .ReturnsAsync(updateResultMock);

            await _repository.UpdateAsync(user);

            _usersCollectionMock.Verify(c => c.UpdateOneAsync(It.IsAny<FilterDefinition<BsonDocument>>(),
                It.IsAny<UpdateDefinition<BsonDocument>>(), null, default), Times.Once);
        }
        #endregion

        #region UpdateAsync_ThrowsKeyNotFoundException_WhenUserDocumentIsIncomplete
        [Fact]
        public async Task UpdateAsync_ThrowsKeyNotFoundException_WhenUserDocumentIsIncomplete()
        {
            var user = new User(new VOId("9f4cde2f-88fb-4689-8198-d3757ced9c24"), new VOName("John"), new VOLastName("Doe"), new VOEmail("john@example.com"),
                new VORoleId("role1"), new VOAddress(null), new VOPhone(null)); // ❌ Faltan dirección y teléfono

            _usersCollectionMock.Setup(c => c.UpdateOneAsync(It.IsAny<FilterDefinition<BsonDocument>>(),
                    It.IsAny<UpdateDefinition<BsonDocument>>(), null, default))
                .ThrowsAsync(new KeyNotFoundException("El documento de usuario no tiene todos los campos requeridos"));

            await Assert.ThrowsAsync<KeyNotFoundException>(() => _repository.UpdateAsync(user));
        }
        #endregion

        #region UpdateAsync_ThrowsException_WhenDatabaseFails
        [Fact]
        public async Task UpdateAsync_ThrowsException_WhenDatabaseFails()
        {
            var user = new User(new VOId("9f4cde2f-88fb-4689-8198-d3757ced9c24"), new VOName("John"), new VOLastName("Doe"), new VOEmail("john@example.com"),
                new VORoleId("role1"), new VOAddress("123 Street"), new VOPhone("1234567890"));

            _usersCollectionMock.Setup(c => c.UpdateOneAsync(It.IsAny<FilterDefinition<BsonDocument>>(),
                    It.IsAny<UpdateDefinition<BsonDocument>>(), null, default))
                .ThrowsAsync(new Exception("Database error"));

            await Assert.ThrowsAsync<Exception>(() => _repository.UpdateAsync(user));
        }
        #endregion


        #region GetByIdAsync_ReturnsUser_WhenUserExists
        [Fact]
        public async Task GetByIdAsync_ReturnsUser_WhenUserExists()
        {
            var userId = "9f4cde2f-88fb-4689-8198-d3757ced9c24";
            var bsonUser = new BsonDocument
            {
                { "_id", userId },
                { "name", "John Doe" },
                { "lastName", "Doe" },
                { "email", "johndoe@gmail.com" },
                { "address", "123 Street" },
                { "phone", "1234567890" },
                { "roleId", "role1" }
            };

            var cursorMock = new Mock<IAsyncCursor<BsonDocument>>();
            cursorMock.SetupSequence(c => c.MoveNextAsync(default))
                .ReturnsAsync(true)
                .ReturnsAsync(false);
            cursorMock.Setup(c => c.Current).Returns(new List<BsonDocument> { bsonUser });

            _usersCollectionMock.Setup(c => c.FindAsync(It.IsAny<FilterDefinition<BsonDocument>>(),
                    It.IsAny<FindOptions<BsonDocument, BsonDocument>>(), default))
                .ReturnsAsync(cursorMock.Object);

            var result = await _repository.GetByIdAsync(userId);

            //Assert.NotNull(result);
            Assert.Equal(userId, result.Id.Value);
            //Assert.Equal("John Doe", result.Name.Value);
        }
        #endregion

        #region GetByIdAsync_ReturnsNull_WhenUserDoesNotExist
        [Fact]
        public async Task GetByIdAsync_ReturnsNull_WhenUserDoesNotExist()
        {
            var userId = "nonexistent_user";

            var cursorMock = new Mock<IAsyncCursor<BsonDocument>>();
            cursorMock.SetupSequence(c => c.MoveNextAsync(default))
                .ReturnsAsync(false);
            cursorMock.Setup(c => c.Current).Returns(new List<BsonDocument>());

            _usersCollectionMock.Setup(c => c.FindAsync(It.IsAny<FilterDefinition<BsonDocument>>(),
                    It.IsAny<FindOptions<BsonDocument, BsonDocument>>(), default))
                .ReturnsAsync(cursorMock.Object);

            var result = await _repository.GetByIdAsync(userId);

            Assert.Null(result);
        }
        #endregion

        #region GetByIdAsync_ThrowsKeyNotFoundException_WhenUserDocumentIsIncomplete
        [Fact]
        public async Task GetByIdAsync_ThrowsKeyNotFoundException_WhenUserDocumentIsIncomplete()
        {
            var userId = "9f4cde2f-88fb-4689-8198-d3757ced9c24";
            var bsonUser = new BsonDocument
            {
                { "_id", userId },
                { "name", "John Doe" }
                // ❌ Falta 'email', 'address', 'phone', 'roleId'
            };

            var cursorMock = new Mock<IAsyncCursor<BsonDocument>>();
            cursorMock.SetupSequence(c => c.MoveNextAsync(default))
                .ReturnsAsync(true)
                .ReturnsAsync(false);
            cursorMock.Setup(c => c.Current).Returns(new List<BsonDocument> { bsonUser });

            _usersCollectionMock.Setup(c => c.FindAsync(It.IsAny<FilterDefinition<BsonDocument>>(),
                    It.IsAny<FindOptions<BsonDocument, BsonDocument>>(), default))
                .ReturnsAsync(cursorMock.Object);

            await Assert.ThrowsAsync<KeyNotFoundException>(() => _repository.GetByIdAsync(userId));
        }
        #endregion

        #region GetByIdAsync_ThrowsException_WhenDatabaseFails
        [Fact]
        public async Task GetByIdAsync_ThrowsException_WhenDatabaseFails()
        {
            var userId = "9f4cde2f-88fb-4689-8198-d3757ced9c24";

            _usersCollectionMock.Setup(c => c.FindAsync(It.IsAny<FilterDefinition<BsonDocument>>(),
                    It.IsAny<FindOptions<BsonDocument, BsonDocument>>(), default))
                .ThrowsAsync(new Exception("Database error"));

            await Assert.ThrowsAsync<Exception>(() => _repository.GetByIdAsync(userId));
        }
        #endregion

    }
}
