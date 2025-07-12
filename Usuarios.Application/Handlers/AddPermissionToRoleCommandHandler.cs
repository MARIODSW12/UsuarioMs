using MassTransit;
using MediatR;
using log4net;

using Usuarios.Application.Commands;

using Usuarios.Domain.Events;
using Usuarios.Domain.Repositories;

namespace Usuarios.Application.Handlers
{
    public class AddPermissionToRoleCommandHandler : IRequestHandler<AddPermissionToRoleCommand, bool>
    {
        private readonly IRoleRepository _roleRepository;
        private readonly IPublishEndpoint _publishEndpoint;
        private static readonly ILog _logger = LogManager.GetLogger(typeof(AddPermissionToRoleCommandHandler));

        public AddPermissionToRoleCommandHandler(IRoleRepository roleRepository, IPublishEndpoint publishEndpoint)
        {
            _roleRepository = roleRepository ?? throw new ArgumentNullException(nameof(roleRepository));
            _publishEndpoint = publishEndpoint ?? throw new ArgumentNullException(nameof(publishEndpoint));
        }

        public async Task<bool> Handle(AddPermissionToRoleCommand request, CancellationToken cancellationToken)
        {
            _logger.Info($"Procesando comando AddPermissionToRole para el rol {request.RoleId} con permiso {request.PermissionId}");

            try
            {
                var role = await _roleRepository.GetByIdAsync(request.RoleId);

                if (role == null)
                {
                    _logger.Warn($"El rol {request.RoleId} no existe.");
                    throw new KeyNotFoundException("El rol no existe.");
                }

                // Agregar el permiso a la lista de permisos del rol
                role.AddPermission(request.PermissionId);

                // Persistir los cambios
                await _roleRepository.AddPermissionsAsync(role.Id, role.PermissionIds);
                _logger.Info($"Permiso {request.PermissionId} agregado exitosamente al rol {role.Id.Value}");

                // Publicar evento de que el permiso se ha agregado al rol
                var eventMessage = new PermissionAddedToRoleEvent(role.Id.Value, request.PermissionId, DateTime.UtcNow);
                await _publishEndpoint.Publish(eventMessage);
                _logger.Info($"Evento publicado: Permiso {request.PermissionId} agregado al rol {role.Id.Value}");

                return true;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error en AddPermissionToRoleCommandHandler para el rol {request.RoleId} con permiso {request.PermissionId}", ex);
                throw;
            }
        }
    }
}
