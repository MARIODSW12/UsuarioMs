using MassTransit;
using MediatR;
using log4net;

namespace Usuarios.Domain.Events.EventHandler
{
    public class PermissionRemovedFromRoleEventHandler : INotificationHandler<PermissionRemovedFromRoleEvent>
    {
        private readonly ISendEndpointProvider _publishEndpoint;
        private static readonly ILog _logger = LogManager.GetLogger(typeof(PermissionRemovedFromRoleEventHandler));

        public PermissionRemovedFromRoleEventHandler(ISendEndpointProvider publishEndpoint)
        {
            _publishEndpoint = publishEndpoint;
        }

        public async Task Handle(PermissionRemovedFromRoleEvent permissionRemovedFromRoleEvent, CancellationToken cancellationToken)
        {
            _logger.Info($"Procesando PermissionRemovedFromRoleEvent para rol ID {permissionRemovedFromRoleEvent.RoleId}");

            try
            {
                await _publishEndpoint.Send(permissionRemovedFromRoleEvent, cancellationToken);
                _logger.Info($"Evento PermissionRemovedFromRole publicado exitosamente en RabbitMQ para rol ID {permissionRemovedFromRoleEvent.RoleId}");
            }
            catch (Exception ex)
            {
                _logger.Error($"Error al manejar PermissionRemovedFromRoleEvent para rol ID {permissionRemovedFromRoleEvent.RoleId}", ex);
                throw;
            }
        }
    }
}
