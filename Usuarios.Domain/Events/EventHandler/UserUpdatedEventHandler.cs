using MassTransit;
using MediatR;
using log4net;

namespace Usuarios.Domain.Events.EventHandler
{
    public class UserUpdatedEventHandler : INotificationHandler<UserUpdatedEvent>
    {
        private readonly ISendEndpointProvider _publishEndpoint;
        private static readonly ILog _logger = LogManager.GetLogger(typeof(UserUpdatedEventHandler));

        public UserUpdatedEventHandler(ISendEndpointProvider publishEndpoint)
        {
            _publishEndpoint = publishEndpoint;
        }

        public async Task Handle(UserUpdatedEvent userUpdatedEvent, CancellationToken cancellationToken)
        {
            _logger.Info($"Procesando UserUpdatedEvent para usuario ID {userUpdatedEvent.UserId}");

            try
            {
                await _publishEndpoint.Send(userUpdatedEvent, cancellationToken);
                _logger.Info($"Evento UserUpdated publicado exitosamente en RabbitMQ para usuario ID {userUpdatedEvent.UserId}");
            }
            catch (Exception ex)
            {
                _logger.Error($"Error al manejar UserUpdatedEvent para usuario ID {userUpdatedEvent.UserId}", ex);
                throw;
            }
        }


    }
}
