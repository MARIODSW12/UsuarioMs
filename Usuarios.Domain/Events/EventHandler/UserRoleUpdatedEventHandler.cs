using MassTransit;
using MediatR;
using log4net;

namespace Usuarios.Domain.Events.EventHandler
{
    public class UserRoleUpdatedEventHandler : INotificationHandler<UserRoleUpdatedEvent>
    {
        private readonly ISendEndpointProvider _publishEndpoint;
        private static readonly ILog _logger = LogManager.GetLogger(typeof(UserRoleUpdatedEventHandler));

        public UserRoleUpdatedEventHandler(ISendEndpointProvider publishEndpoint)
        {
            _publishEndpoint = publishEndpoint;
        }

        public async Task Handle(UserRoleUpdatedEvent userUpdatedRoleEvent, CancellationToken cancellationToken)
        {
            _logger.Info($"Procesando UserRoleUpdatedEvent para usuario ID {userUpdatedRoleEvent.UserId}");

            try
            {
                await _publishEndpoint.Send(userUpdatedRoleEvent, cancellationToken);
                _logger.Info($"Evento UserRoleUpdated publicado exitosamente en RabbitMQ para usuario ID {userUpdatedRoleEvent.UserId}");
            }
            catch (Exception ex)
            {
                _logger.Error($"Error al manejar UserRoleUpdatedEvent para usuario ID {userUpdatedRoleEvent.UserId}", ex);
                throw;
            }
        }

    }
}
