using MongoDB.Bson;
using MongoDB.Driver;
using Moq;

using Usuarios.Domain.Repositories;

using Usuarios.Infrastructure.Configurations;
using Usuarios.Infrastructure.Persistence.Repository.MongoRead;

namespace Usuarios.Tests.Usuarios.Infrastructure.Persistence.Repository.MongoRead
{
    public class MongoReadUserActivityRepositoryTests
    {
        private readonly Mock<IMongoDatabase> _mongoDatabaseMock;
        private readonly Mock<IMongoCollection<BsonDocument>> _userActivityCollectionMock;
        private readonly MongoReadUserActivityRepository _repository;
        public MongoReadUserActivityRepositoryTests()
        {
            _mongoDatabaseMock = new Mock<IMongoDatabase>();
            _userActivityCollectionMock = new Mock<IMongoCollection<BsonDocument>>();

            _mongoDatabaseMock.Setup(d => d.GetCollection<BsonDocument>("Historico", It.IsAny<MongoCollectionSettings>()))
                              .Returns(_userActivityCollectionMock.Object);

            Environment.SetEnvironmentVariable("MONGODB_CNN_READ_ACTIVITY", "mongodb://localhost:27017");
            Environment.SetEnvironmentVariable("MONGODB_NAME_READ_ACTIVITY", "test_database");
            var mongoConfigMock = new MongoReadUserActivityDbConfig();
            mongoConfigMock.db = _mongoDatabaseMock.Object;

            _repository = new MongoReadUserActivityRepository(mongoConfigMock, Mock.Of<IRoleRepository>());
        }

        #region AddAsync_InsertsActivityCorrectly_WhenValidActivityProvided
        [Fact]
        public async Task AddAsync_InsertsActivityCorrectly_WhenValidActivityProvided()
        {
            var userActivity = new BsonDocument
            {
                { "_userid", "user123" },
                { "message", "Logged in" },
                { "timestamp", DateTime.UtcNow }
            };
            _userActivityCollectionMock.Setup(c => c.InsertOneAsync(userActivity, null, default))
                .Returns(Task.CompletedTask);

            await _repository.AddAsync(userActivity);

            _userActivityCollectionMock.Verify(c => c.InsertOneAsync(userActivity, null, default), Times.Once);
        }
        #endregion

        #region AddAsync_ThrowsException_WhenInsertFails
        [Fact]
        public async Task AddAsync_ThrowsException_WhenInsertFails()
        {
            var userActivity = new BsonDocument
            {
                { "_userid", "user123" },
                { "message", "Logged in" },
                { "timestamp", DateTime.UtcNow }
            };

            _userActivityCollectionMock.Setup(c => c.InsertOneAsync(It.IsAny<BsonDocument>(), null, default))
                .ThrowsAsync(new Exception("Database error"));

            await Assert.ThrowsAsync<Exception>(() => _repository.AddAsync(userActivity));
        }
        #endregion


        #region GetAllAsync_ReturnsActivityList_WhenActivitiesExist
        [Fact]
        public async Task GetAllAsync_ReturnsActivityList_WhenActivitiesExist()
        {
            var activities = new List<BsonDocument>
            {
                new BsonDocument { { "_userid", "user1" }, { "message", "Login" }, { "timestamp", DateTime.UtcNow } },
                new BsonDocument { { "_userid", "user2" }, { "message", "Logout" }, { "timestamp", DateTime.UtcNow } }
            };

            var cursorMock = new Mock<IAsyncCursor<BsonDocument>>();
            cursorMock.SetupSequence(c => c.MoveNextAsync(default))
                .ReturnsAsync(true)
                .ReturnsAsync(false);
            cursorMock.Setup(c => c.Current).Returns(activities);

            _userActivityCollectionMock.Setup(c => c.FindAsync(It.IsAny<FilterDefinition<BsonDocument>>(), It.IsAny<FindOptions<BsonDocument, BsonDocument>>(), default))
                .ReturnsAsync(cursorMock.Object);

            var result = await _repository.GetAllAsync();

            Assert.Equal(2, result.Count);
        }
        #endregion

        #region GetAllAsync_ReturnsEmptyList_WhenNoActivitiesExist
        [Fact]
        public async Task GetAllAsync_ReturnsEmptyList_WhenNoActivitiesExist()
        {
            var cursorMock = new Mock<IAsyncCursor<BsonDocument>>();
            cursorMock.SetupSequence(c => c.MoveNextAsync(default))
                .ReturnsAsync(false);
            cursorMock.Setup(c => c.Current).Returns(new List<BsonDocument>());

            _userActivityCollectionMock.Setup(c => c.FindAsync(It.IsAny<FilterDefinition<BsonDocument>>(), It.IsAny<FindOptions<BsonDocument, BsonDocument>>(), default))
                .ReturnsAsync(cursorMock.Object);

            var result = await _repository.GetAllAsync();

            Assert.Empty(result);
        }
        #endregion

        #region GetAllAsync_ThrowsException_WhenDatabaseFails
        [Fact]
        public async Task GetAllAsync_ThrowsException_WhenDatabaseFails()
        {
            _userActivityCollectionMock.Setup(c => c.FindAsync(It.IsAny<FilterDefinition<BsonDocument>>(), It.IsAny<FindOptions<BsonDocument, BsonDocument>>(), default))
                .ThrowsAsync(new Exception("Database error"));

            await Assert.ThrowsAsync<Exception>(() => _repository.GetAllAsync());
        }
        #endregion


        #region GetByUserIdAsync_ReturnsUserActivities_WhenActivitiesExist
        [Fact]
        public async Task GetByUserIdAsync_ReturnsUserActivities_WhenActivitiesExist()
        {
            var userId = "user123";
            var timestamp = DateTime.UtcNow.AddHours(-1);
            var activities = new List<BsonDocument>
        {
            new BsonDocument { { "_userid", userId }, { "message", "Login" }, { "timestamp", DateTime.UtcNow } }
        };

            var cursorMock = new Mock<IAsyncCursor<BsonDocument>>();
            cursorMock.SetupSequence(c => c.MoveNextAsync(default))
                      .ReturnsAsync(true)
                      .ReturnsAsync(false);
            cursorMock.Setup(c => c.Current).Returns(activities);

            _userActivityCollectionMock.Setup(c => c.FindAsync(It.IsAny<FilterDefinition<BsonDocument>>(), It.IsAny<FindOptions<BsonDocument, BsonDocument>>(), default))
                                       .ReturnsAsync(cursorMock.Object);

            var result = await _repository.GetByUserIdAsync(userId, timestamp);

            Assert.Single(result);
        }
        #endregion

        #region GetByUserIdAsync_ReturnsEmptyList_WhenNoActivitiesExist
        [Fact]
        public async Task GetByUserIdAsync_ReturnsEmptyList_WhenNoActivitiesExist()
        {
            var userId = "user123";
            var timestamp = DateTime.UtcNow.AddHours(-1);

            var cursorMock = new Mock<IAsyncCursor<BsonDocument>>();
            cursorMock.SetupSequence(c => c.MoveNextAsync(default))
                .ReturnsAsync(false);
            cursorMock.Setup(c => c.Current).Returns(new List<BsonDocument>());

            _userActivityCollectionMock.Setup(c => c.FindAsync(It.IsAny<FilterDefinition<BsonDocument>>(), It.IsAny<FindOptions<BsonDocument, BsonDocument>>(), default))
                .ReturnsAsync(cursorMock.Object);

            var result = await _repository.GetByUserIdAsync(userId, timestamp);

            Assert.Empty(result);
        }
        #endregion

        #region GetByUserIdAsync_ReturnsEmptyList_WhenUserIdDoesNotExist
        [Fact]
        public async Task GetByUserIdAsync_ReturnsEmptyList_WhenUserIdDoesNotExist()
        {
            var userId = "nonExistentUser";
            var timestamp = DateTime.UtcNow;

            var cursorMock = new Mock<IAsyncCursor<BsonDocument>>();
            cursorMock.SetupSequence(c => c.MoveNextAsync(default))
                .ReturnsAsync(false);
            cursorMock.Setup(c => c.Current).Returns(new List<BsonDocument>());

            _userActivityCollectionMock.Setup(c => c.FindAsync(It.IsAny<FilterDefinition<BsonDocument>>(), It.IsAny<FindOptions<BsonDocument, BsonDocument>>(), default))
                .ReturnsAsync(cursorMock.Object);

            var result = await _repository.GetByUserIdAsync(userId, timestamp);

            Assert.Empty(result);
        }
        #endregion

        #region GetByUserIdAsync_ThrowsException_WhenDatabaseFails
        [Fact]
        public async Task GetByUserIdAsync_ThrowsException_WhenDatabaseFails()
        {
            var userId = "user123";
            var timestamp = DateTime.UtcNow;

            _userActivityCollectionMock.Setup(c => c.FindAsync(It.IsAny<FilterDefinition<BsonDocument>>(), It.IsAny<FindOptions<BsonDocument, BsonDocument>>(), default))
                .ThrowsAsync(new Exception("Database error"));

            await Assert.ThrowsAsync<Exception>(() => _repository.GetByUserIdAsync(userId, timestamp));
        }
        #endregion

    }
}
