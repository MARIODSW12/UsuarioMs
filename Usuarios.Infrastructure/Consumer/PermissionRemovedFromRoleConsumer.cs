using MassTransit;
using MongoDB.Bson;
using log4net;

using Usuarios.Domain.Events;

using Usuarios.Infrastructure.Interfaces;

namespace Usuarios.Infrastructure.Consumer
{
    public class PermissionRemovedFromRoleConsumer(IServiceProvider serviceProvider, IRoleReadRepository rolereadRepository) : IConsumer<PermissionRemovedFromRoleEvent>
    {
        private readonly IServiceProvider _serviceProvider = serviceProvider;
        private readonly IRoleReadRepository _roleReadRepository = rolereadRepository;
        private static readonly ILog _logger = LogManager.GetLogger(typeof(PermissionRemovedFromRoleConsumer));

        public async Task Consume(ConsumeContext<PermissionRemovedFromRoleEvent> @event)
        {
            _logger.Info($"Procesando PermissionRemovedFromRoleEvent para rol ID {@event.Message.RoleId}, permiso ID {@event.Message.PermissionId}");

            try
            {
                var message = @event.Message;

                var bsonRole = new BsonDocument
                {
                    { "_id", message.RoleId },
                    { "PermissionIds", message.PermissionId }
                };

                await _roleReadRepository.RemovePermissionByIdAsync(message.RoleId, message.PermissionId);
                _logger.Info($"Permiso ID {message.PermissionId} eliminado exitosamente del rol ID {message.RoleId}");
            }
            catch (Exception ex)
            {
                _logger.Error($"Error al procesar PermissionRemovedFromRoleEvent para rol ID {@event.Message.RoleId}, permiso ID {@event.Message.PermissionId}", ex);
                throw;
            }
        }
    }
}
