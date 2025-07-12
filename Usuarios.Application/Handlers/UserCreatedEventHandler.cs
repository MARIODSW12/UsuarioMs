using MassTransit;
using MediatR;
using log4net;

using Usuarios.Domain.Events;

namespace Usuarios.Application.EventHandlers
{
    public class UserCreatedEventHandler : INotificationHandler<UserCreatedEvent>
    {
        private readonly ISendEndpointProvider _publishEndpoint;
        private static readonly ILog _logger = LogManager.GetLogger(typeof(UserCreatedEventHandler));

        public UserCreatedEventHandler(ISendEndpointProvider publishEndpoint)
        {
            _publishEndpoint = publishEndpoint;
        }

        public async Task Handle(UserCreatedEvent userCreatedEvent, CancellationToken cancellationToken)
        {
            _logger.Info($"Procesando UserCreatedEvent para usuario ID {userCreatedEvent.Id}");

            try
            {
                await _publishEndpoint.Send(userCreatedEvent, cancellationToken);
                _logger.Info($"Evento UserCreated publicado exitosamente en RabbitMQ para usuario ID {userCreatedEvent.Id}");
            }
            catch (Exception ex)
            {
                _logger.Error($"Error al manejar UserCreatedEvent para usuario ID {userCreatedEvent.Id}", ex);
                throw;
            }
        }
    }
}