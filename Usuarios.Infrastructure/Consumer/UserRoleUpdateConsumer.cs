using MassTransit;
using MongoDB.Bson;
using log4net;

using Usuarios.Domain.Events;

using Usuarios.Infrastructure.Interfaces;

namespace Usuarios.Infrastructure.Consumer
{
    public class UserRoleUpdateConsumer(IServiceProvider serviceProvider, IUserReadRepository userReadRepository) : IConsumer<UserRoleUpdatedEvent>
    {
        private readonly IServiceProvider _serviceProvider = serviceProvider;
        private readonly IUserReadRepository _userReadRepository = userReadRepository;
        private static readonly ILog _logger = LogManager.GetLogger(typeof(UserRoleUpdateConsumer));
        public async Task Consume(ConsumeContext<UserRoleUpdatedEvent> @event)
        {
            _logger.Info($"Procesando UserRoleUpdatedEvent para usuario ID {@event.Message.UserId}");

            try
            {
                var message = @event.Message;

                var bsonUser = new BsonDocument
                {
                    { "_id", message.UserId },
                    { "roleId", message.NewRoleId } // Agregado para actualizar el RoleId
                };

                await _userReadRepository.UpdateRoleIdById(bsonUser);
                _logger.Info($"Usuario ID {message.UserId} actualizado exitosamente con nuevo RoleId {message.NewRoleId}");
            }
            catch (Exception ex)
            {
                _logger.Error($"Error al procesar UserRoleUpdatedEvent para usuario ID {@event.Message.UserId}", ex);
                throw;
            }
        }
    }
}
