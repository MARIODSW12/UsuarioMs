using log4net;
using MassTransit;
using MediatR;

using Usuarios.Application.Commands;

using Usuarios.Domain.Events;
using Usuarios.Domain.Repositories;

namespace Usuarios.Application.Handlers
{
    public class RemovePermissionFromRoleCommandHandler : IRequestHandler<RemovePermissionFromRoleCommand, bool>
    {
        private readonly IRoleRepository _roleRepository;
        private readonly IPublishEndpoint _publishEndpoint;
        private static readonly ILog _logger = LogManager.GetLogger(typeof(RemovePermissionFromRoleCommandHandler));

        public RemovePermissionFromRoleCommandHandler(IRoleRepository roleRepository, IPublishEndpoint publishEndpoint)
        {
            _roleRepository = roleRepository ?? throw new ArgumentNullException(nameof(roleRepository));
            _publishEndpoint = publishEndpoint ?? throw new ArgumentNullException(nameof(publishEndpoint));
        }

        public async Task<bool> Handle(RemovePermissionFromRoleCommand request, CancellationToken cancellationToken)
        {
            _logger.Info($"Procesando comando RemovePermissionFromRole para el rol {request.RoleId} con permiso {request.PermissionId}");

            try
            {
                var role = await _roleRepository.GetByIdAsync(request.RoleId);

                if (role == null)
                {
                    _logger.Warn($"El rol {request.RoleId} no existe.");
                    throw new KeyNotFoundException("El rol no existe.");
                }

                // Remover el permiso de la lista de permisos del rol
                role.RemovePermission(request.PermissionId);
                _logger.Info($"Permiso {request.PermissionId} removido del rol {role.Id.Value}");

                // Persistir los cambios
                await _roleRepository.UpdateAsync(role);
                _logger.Info($"Cambios persistidos en el rol {role.Id.Value}");

                // Publicar evento de que el permiso se ha eliminado del rol
                var eventMessage = new PermissionRemovedFromRoleEvent(role.Id.Value, request.PermissionId, DateTime.UtcNow);
                await _publishEndpoint.Publish(eventMessage);
                _logger.Info($"Evento publicado: Permiso {request.PermissionId} eliminado del rol {role.Id.Value}");

                return true;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error en RemovePermissionFromRoleCommandHandler para el rol {request.RoleId} con permiso {request.PermissionId}", ex);
                throw;
            }
        }
    }
}
