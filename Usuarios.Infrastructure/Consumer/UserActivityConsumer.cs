using MassTransit;
using MongoDB.Bson;
using log4net;

using Usuarios.Application.Events;

using Usuarios.Infrastructure.Interfaces;

namespace Usuarios.Infrastructure.Consumer
{
    public class UserActivityConsumer(IServiceProvider serviceProvider, IUserActivityReadRepository userActivityReadRepository) : IConsumer<UserActivityMadeEvent>
    {
        private readonly IServiceProvider _serviceProvider = serviceProvider;
        private readonly IUserActivityReadRepository _userActivityReadRepository = userActivityReadRepository;
        private static readonly ILog _logger = LogManager.GetLogger(typeof(UserActivityConsumer));

        public async Task Consume(ConsumeContext<UserActivityMadeEvent> @event)
        {
            _logger.Info($"Procesando UserActivityMadeEvent para usuario ID {@event.Message.UserId}");

            try
            {
                var message = @event.Message;

                var bsonUser = new BsonDocument
                {
                    { "_id", Guid.NewGuid().ToString() },
                    { "_userid", message.UserId },
                    { "message", message.Action },
                    { "timestamp", message.Timestamp }
                };

                await _userActivityReadRepository.AddAsync(bsonUser);
                _logger.Info($"Actividad de usuario ID {message.UserId} guardada exitosamente en la base de datos.");
            }
            catch (Exception ex)
            {
                _logger.Error($"Error al procesar UserActivityMadeEvent para usuario ID {@event.Message.UserId}", ex);
                throw;
            }
        }
    }
}
