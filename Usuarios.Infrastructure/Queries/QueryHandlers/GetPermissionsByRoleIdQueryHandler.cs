using log4net;
using MediatR;
using MongoDB.Bson;
using MongoDB.Driver;
using Usuarios.Infrastructure.Interfaces;
using Usuarios.Infrastructure.Persistence.Repository.MongoRead;

namespace Usuarios.Infrastructure.Queries.QueryHandlers
{
    public class GetPermissionsByRoleIdQueryHandler : IRequestHandler<GetPermissionsByRoleIdQuery, List<string>?>
    {
        private readonly IRoleReadRepository _roleReadRepository;
        private static readonly ILog _logger = LogManager.GetLogger(typeof(GetPermissionsByRoleIdQueryHandler));

        public GetPermissionsByRoleIdQueryHandler(IRoleReadRepository roleReadRepository)
        {
            _roleReadRepository = roleReadRepository ?? throw new ArgumentNullException(nameof(roleReadRepository));
        }

        public async Task<List<string>?> Handle(GetPermissionsByRoleIdQuery request, CancellationToken cancellationToken)
        {
            _logger.Info($"Procesando consulta GetPermissionsByRoleIdQuery para el rol {request.RoleId}");

            try
            {
                var permissions = await _roleReadRepository.GetPermissionsByRoleIdAsync(request.RoleId);

                if (permissions == null)
                {
                    _logger.Warn($"No se encontró rol con ID {request.RoleId}");
                    return null;
                }

                var permissionids = permissions.ToList() ?? new List<string>();

                _logger.Info($"Consulta completada. Permisos del rol {request.RoleId}: {string.Join(", ", permissionids)}");
                return permissionids;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error en GetPermissionsByRoleIdQueryHandler al obtener permisos del rol {request.RoleId}", ex);
                throw;
            }
        }
    }
}
