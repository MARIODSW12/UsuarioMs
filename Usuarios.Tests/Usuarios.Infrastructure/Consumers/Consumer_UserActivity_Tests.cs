using MassTransit;
using MongoDB.Bson;
using Moq;

using Usuarios.Application.Events;

using Usuarios.Infrastructure.Consumer;
using Usuarios.Infrastructure.Interfaces;

namespace Usuarios.Tests.Usuarios.Infrastructure.Consumers
{
    public class UserActivityConsumerTests
    {
        private readonly Mock<IUserActivityReadRepository> _userActivityReadRepositoryMock;
        private readonly Mock<IServiceProvider> _serviceProviderMock;
        private readonly UserActivityConsumer _consumer;

        public UserActivityConsumerTests()
        {
            _userActivityReadRepositoryMock = new Mock<IUserActivityReadRepository>();
            _serviceProviderMock = new Mock<IServiceProvider>();

            _consumer = new UserActivityConsumer(_serviceProviderMock.Object, _userActivityReadRepositoryMock.Object);
        }

        #region Consume_AddsActivityToDatabase_WhenEventIsProcessedSuccessfully
        [Fact]
        public async Task Consume_AddsActivityToDatabase_WhenEventIsProcessedSuccessfully()
        {
            var message = new UserActivityMadeEvent
            (
                "user123",
                "Logged in",
                DateTime.UtcNow
            );

            var contextMock = new Mock<ConsumeContext<UserActivityMadeEvent>>();
            contextMock.Setup(c => c.Message).Returns(message);

            _userActivityReadRepositoryMock.Setup(r => r.AddAsync(It.IsAny<BsonDocument>())).Returns(Task.CompletedTask);

            await _consumer.Consume(contextMock.Object);

            _userActivityReadRepositoryMock.Verify(r => r.AddAsync(It.Is<BsonDocument>(bson =>
                bson["_userid"] == message.UserId &&
                bson["message"] == message.Action &&
                bson["timestamp"] == message.Timestamp
            )), Times.Once);
        }
        #endregion

        #region Consume_StoresCorrectTimestampPrecision_WhenEventIsProcessed
        [Fact]
        public async Task Consume_StoresCorrectTimestampPrecision_WhenEventIsProcessed()
        {
            var timestamp = DateTime.UtcNow;
            var message = new UserActivityMadeEvent
            (
                "user123",
                "Logged in",
                timestamp
            );

            var contextMock = new Mock<ConsumeContext<UserActivityMadeEvent>>();
            contextMock.Setup(c => c.Message).Returns(message);

            _userActivityReadRepositoryMock.Setup(r => r.AddAsync(It.IsAny<BsonDocument>())).Returns(Task.CompletedTask);

            await _consumer.Consume(contextMock.Object);

            _userActivityReadRepositoryMock.Verify(r => r.AddAsync(It.Is<BsonDocument>(bson =>
                bson["timestamp"].ToUniversalTime().Ticks / TimeSpan.TicksPerSecond ==
                timestamp.ToUniversalTime().Ticks / TimeSpan.TicksPerSecond
            )), Times.Once);
        }
        #endregion

        #region Consume_ThrowsException_WhenRepositoryFails
        [Fact]
        public async Task Consume_ThrowsException_WhenRepositoryFails()
        {
            var message = new UserActivityMadeEvent
            (
                "user123",
                "Logged in",
                DateTime.UtcNow
            );

            var contextMock = new Mock<ConsumeContext<UserActivityMadeEvent>>();
            contextMock.Setup(c => c.Message).Returns(message);

            _userActivityReadRepositoryMock.Setup(r => r.AddAsync(It.IsAny<BsonDocument>()))
                .ThrowsAsync(new Exception("Database error"));

            await Assert.ThrowsAsync<Exception>(() => _consumer.Consume(contextMock.Object));
        }
        #endregion

    }
}
