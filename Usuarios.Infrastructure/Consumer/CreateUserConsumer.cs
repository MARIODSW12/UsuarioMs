using MassTransit;
using MongoDB.Bson;
using log4net;

using Usuarios.Domain.Events;

using Usuarios.Infrastructure.Interfaces;

namespace Usuarios.Infrastructure.Consumer
{
    public class CreateUserConsumer(IServiceProvider serviceProvider, IUserReadRepository userReadRepository) : IConsumer<UserCreatedEvent>
    {
        private readonly IServiceProvider _serviceProvider = serviceProvider;
        private readonly IUserReadRepository _userReadRepository = userReadRepository;
        private static readonly ILog _logger = LogManager.GetLogger(typeof(CreateUserConsumer));

        public async Task Consume(ConsumeContext<UserCreatedEvent> @event)
        {
            _logger.Info($"Procesando UserCreatedEvent para usuario ID {@event.Message.Id}");

            try
            {
                var message = @event.Message;
                var bsonUser = new BsonDocument
                {
                    { "_id", message.Id },
                    { "name", message.Name },
                    { "lastName", message.LastName },
                    { "email", message.Email },
                    { "address", message.Address != null ? new BsonString(message.Address) : BsonNull.Value },
                    { "phone", message.Phone != null ? new BsonString(message.Phone) : BsonNull.Value },
                    { "roleId", message.RoleId }
                };

                await _userReadRepository.AddAsync(bsonUser);
                _logger.Info($"Usuario ID {message.Id} agregado exitosamente a la base de datos.");
            }
            catch (Exception ex)
            {
                _logger.Error($"Error al procesar UserCreatedEvent para usuario ID {@event.Message.Id}", ex);
                throw;
            }
        }
    }
}