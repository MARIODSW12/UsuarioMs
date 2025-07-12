using MassTransit;
using MediatR;
using log4net;

namespace Usuarios.Domain.Events.EventHandler
{
    public class PermissionAddedToRoleEventHandler : INotificationHandler<PermissionAddedToRoleEvent>
    {

        private readonly ISendEndpointProvider _publishEndpoint;
        private static readonly ILog _logger = LogManager.GetLogger(typeof(PermissionAddedToRoleEventHandler));

        public PermissionAddedToRoleEventHandler(ISendEndpointProvider publishEndpoint)
        {
            _publishEndpoint = publishEndpoint;
        }

        public async Task Handle(PermissionAddedToRoleEvent permissionAddedToRoleEvent, CancellationToken cancellationToken)
        {
            _logger.Info($"Procesando PermissionAddedToRoleEvent para rol ID {permissionAddedToRoleEvent.RoleId}");

            try
            {
                await _publishEndpoint.Send(permissionAddedToRoleEvent, cancellationToken);
                _logger.Info($"Evento PermissionAddedToRole publicado exitosamente en RabbitMQ para rol ID {permissionAddedToRoleEvent.RoleId}");
            }
            catch (Exception ex)
            {
                _logger.Error($"Error al manejar PermissionAddedToRoleEvent para rol ID {permissionAddedToRoleEvent.RoleId}", ex);
                throw;
            }
        }
    }
}
