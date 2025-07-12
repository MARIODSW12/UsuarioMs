using log4net;
using MediatR;
using MassTransit;

using Usuarios.Application.Commands;
using Usuarios.Application.Events;

using Usuarios.Domain.Repositories;
using Usuarios.Domain.Events;

namespace Usuarios.Application.Handlers
{
    public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, bool>
    {
        private readonly IUserRepository _userRepository;
        private readonly IPublishEndpoint _publishEndpoint;
        private static readonly ILog _logger = LogManager.GetLogger(typeof(UpdateUserCommandHandler));

        public UpdateUserCommandHandler(IUserRepository userRepository, IPublishEndpoint publishEndpoint)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _publishEndpoint = publishEndpoint ?? throw new ArgumentNullException(nameof(publishEndpoint));
        }

        public async Task<bool> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
        {
            _logger.Info($"Procesando comando UpdateUser para el usuario {request.id}");

            try
            {
                var user = await _userRepository.GetByIdAsync(request.id);

                if (user == null)
                {
                    _logger.Warn($"El usuario {request.id} no existe.");
                    throw new ArgumentException("El usuario no existe.");
                }

                user.Update(
                    new string(request.UserDto.Name),
                    new string(request.UserDto.LastName),
                    new string(request.UserDto.Address),
                    new string(request.UserDto.Phone)
                );

                _logger.Info($"Ejecutando actualización en MongoWriteUserRepository para el usuario {request.id}");
                await _userRepository.UpdateAsync(user);
                _logger.Info($"Usuario {request.id} actualizado correctamente.");

                var userUpdatedEvent = new UserUpdatedEvent(user.Id, user.Name, user.LastName, user.Address, user.Phone);
                await _publishEndpoint.Publish(userUpdatedEvent);
                _logger.Info($"Evento publicado: Usuario {user.Id.Value} actualizado.");

                await _publishEndpoint.Publish(new UserActivityMadeEvent(user.Id.Value, "USER_UPDATED", DateTime.UtcNow));
                _logger.Info($"Evento de actividad publicado: Usuario {user.Id.Value} actualizado.");

                return true;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error en UpdateUserCommandHandler para el usuario {request.id}", ex);
                throw;
            }
        }
    }
}