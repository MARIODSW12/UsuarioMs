using MassTransit;
using MediatR;
using log4net;

using Usuarios.Application.Commands;

using Usuarios.Domain.Events;
using Usuarios.Domain.Repositories;

namespace Usuarios.Application.Handlers
{
    public class UpdateUserRoleCommandHandler : IRequestHandler<UpdateUserRoleCommand, bool>
    {
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IPublishEndpoint _publishEndpoint;
        private static readonly ILog _logger = LogManager.GetLogger(typeof(UpdateUserRoleCommandHandler));

        public UpdateUserRoleCommandHandler(IUserRepository userRepository, IPublishEndpoint publishEndpoint, IRoleRepository roleRepository)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _roleRepository = roleRepository ?? throw new ArgumentNullException(nameof(roleRepository));
            _publishEndpoint = publishEndpoint ?? throw new ArgumentNullException(nameof(publishEndpoint));
        }

        public async Task<bool> Handle(UpdateUserRoleCommand request, CancellationToken cancellationToken)
        {
            _logger.Info($"Procesando comando UpdateUserRole para el usuario {request.UserId}");

            try
            {
                // Obtiene al usuario a través del repositorio
                var user = await _userRepository.GetByIdAsync(request.UserId);
                if (user == null)
                {
                    _logger.Warn($"El usuario {request.UserId} no existe.");
                    throw new ArgumentException("El usuario no existe.");
                }

                // Validar que el rol a asignar existe
                var role = await _roleRepository.GetByIdAsync(request.UserRoleDto.NewRoleId);
                if (role == null)
                {
                    _logger.Warn($"El rol {request.UserRoleDto.NewRoleId} no existe.");
                    throw new ArgumentException("El rol a asignar no existe.");
                }

                // Actualizar el rol del usuario
                user.UpdateRole(request.UserRoleDto.NewRoleId);
                _logger.Info($"Rol del usuario {request.UserId} actualizado a {request.UserRoleDto.NewRoleId}");

                await _userRepository.UpdateAsync(user);
                _logger.Info($"Cambios persistidos en la base de datos para el usuario {request.UserId}");

                // Publicar evento de actualización de rol
                var userRoleUpdatedEvent = new UserRoleUpdatedEvent(user.Id.Value, user.RoleId.Value);
                await _publishEndpoint.Publish(userRoleUpdatedEvent);
                _logger.Info($"Evento publicado: Usuario {user.Id.Value} actualizado con nuevo rol {user.RoleId.Value}");

                return true;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error en UpdateUserRoleCommandHandler para el usuario {request.UserId}", ex);
                throw;
            }
        }
    }
}
