using MassTransit;
using MongoDB.Bson;
using log4net;

using Usuarios.Domain.Events;

using Usuarios.Infrastructure.Interfaces;

namespace Usuarios.Infrastructure.Consumer
{
    public class PermissionAddedToRoleConsumer(IServiceProvider serviceProvider, IRoleReadRepository rolereadRepository) : IConsumer<PermissionAddedToRoleEvent>
    {
        private readonly IServiceProvider _serviceProvider = serviceProvider;
        private readonly IRoleReadRepository _roleReadRepository = rolereadRepository;
        private static readonly ILog _logger = LogManager.GetLogger(typeof(PermissionAddedToRoleConsumer));

        public async Task Consume(ConsumeContext<PermissionAddedToRoleEvent> @event)
        {
            _logger.Info($"Procesando PermissionAddedToRoleEvent para rol ID {@event.Message.RoleId}, permiso ID {@event.Message.PermissionId}");

            try
            {
                var message = @event.Message;

                var bsonRole = new BsonDocument
                {
                    { "_id", message.RoleId },
                    { "PermissionIds", message.PermissionId }
                };

                await _roleReadRepository.AddPermissionsAsync(bsonRole);
                _logger.Info($"Permiso ID {message.PermissionId} agregado exitosamente al rol ID {message.RoleId}");
            }
            catch (Exception ex)
            {
                _logger.Error($"Error al procesar PermissionAddedToRoleEvent para rol ID {@event.Message.RoleId}, permiso ID {@event.Message.PermissionId}", ex);
                throw;
            }
        }
    }
}
