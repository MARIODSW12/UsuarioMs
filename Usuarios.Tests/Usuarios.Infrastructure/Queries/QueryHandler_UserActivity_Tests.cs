using MongoDB.Bson;
using Moq;

using Usuarios.Infrastructure.Interfaces;
using Usuarios.Infrastructure.Queries;
using Usuarios.Infrastructure.Queries.QueryHandlers;

namespace Usuarios.Tests.Usuarios.Infrastructure.Queries
{
    public class UserActivityQueryHandlerTests
    {
        private readonly Mock<IUserActivityReadRepository> _userActivityReadRepositoryMock;
        private readonly UserActivityQueryHandler _handler;

        public UserActivityQueryHandlerTests()
        {
            _userActivityReadRepositoryMock = new Mock<IUserActivityReadRepository>();
            _handler = new UserActivityQueryHandler(_userActivityReadRepositoryMock.Object);
        }

        #region Handle_ReturnsUserActivities_WhenActivitiesExist
        [Fact]
        public async Task Handle_ReturnsUserActivities_WhenActivitiesExist()
        {
            var query = new UserActivityQuery("user123", DateTime.Today);
            var activities = new List<BsonDocument>
        {
            new BsonDocument
            {
                { "_id", "act1" },
                { "_userid", "user123" },
                { "message", "Logged in" },
                { "timestamp", DateTime.UtcNow }
            },
            new BsonDocument
            {
                { "_id", "act2" },
                { "_userid", "user123" },
                { "message", "Viewed dashboard" },
                { "timestamp", DateTime.UtcNow }
            }
        };

            _userActivityReadRepositoryMock.Setup(r => r.GetByUserIdAsync(query.UserId, DateTime.Today))
                .ReturnsAsync(activities);

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.Equal(2, result.Count());
        }
        #endregion

        #region Handle_ReturnsEmptyList_WhenNoActivitiesExist
        [Fact]
        public async Task Handle_ReturnsEmptyList_WhenNoActivitiesExist()
        {
            var query = new UserActivityQuery("user123", DateTime.Today);

            _userActivityReadRepositoryMock.Setup(r => r.GetByUserIdAsync(query.UserId, DateTime.Today))
                .ReturnsAsync(new List<BsonDocument>()); // Simulación de lista vacía

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.Empty(result);
        }
        #endregion

        #region Handle_ThrowsException_WhenRepositoryFails
        [Fact]
        public async Task Handle_ThrowsException_WhenRepositoryFails()
        {
            var query = new UserActivityQuery("user123", DateTime.Today);

            _userActivityReadRepositoryMock.Setup(r => r.GetByUserIdAsync(query.UserId, DateTime.Today))
                .ThrowsAsync(new Exception("Database error"));

            await Assert.ThrowsAsync<Exception>(() => _handler.Handle(query, CancellationToken.None));
        }
        #endregion

        #region Handle_ReturnsCorrectTimestamp_WhenActivitiesExist
        [Fact]
        public async Task Handle_ReturnsCorrectTimestamp_WhenActivitiesExist()
        {
            var query = new UserActivityQuery("user123", DateTime.Today);
            var timestamp = DateTime.UtcNow;
            var activities = new List<BsonDocument>
        {
            new BsonDocument
            {
                { "_id", "act1" },
                { "_userid", "user123" },
                { "message", "Logged in" },
                { "timestamp", timestamp }
            }
        };

            _userActivityReadRepositoryMock.Setup(r => r.GetByUserIdAsync(query.UserId, DateTime.Today))
                .ReturnsAsync(activities);

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.Equal(timestamp.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss"),
                result.First().Timestamp.ToString("yyyy-MM-dd HH:mm:ss"));
        }
        #endregion

    }
}
