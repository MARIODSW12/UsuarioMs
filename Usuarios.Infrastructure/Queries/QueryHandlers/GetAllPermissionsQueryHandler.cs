using MediatR;
using log4net;

using Usuarios.Application.DTOs;

using Usuarios.Infrastructure.Interfaces;

namespace Usuarios.Infrastructure.Queries.QueryHandlers
{
    public class GetAllPermissionsQueryHandler : IRequestHandler<GetAllPermissionsQuery, List<PermissionDto>>
    {
        private readonly IPermissionReadRepository _permissionReadRepository;
        private static readonly ILog _logger = LogManager.GetLogger(typeof(GetAllPermissionsQueryHandler));

        public GetAllPermissionsQueryHandler(IPermissionReadRepository permissionReadRepository)
        {
            _permissionReadRepository = permissionReadRepository ?? throw new ArgumentNullException(nameof(permissionReadRepository));
        }

        public async Task<List<PermissionDto>> Handle(GetAllPermissionsQuery request, CancellationToken cancellationToken)
        {
            _logger.Info("Procesando consulta GetAllPermissionsQuery");

            try
            {
                var permissions = await _permissionReadRepository.GetAllAsync();

                if (permissions == null || !permissions.Any())
                {
                    _logger.Warn("No se encontraron permisos en la base de datos.");
                    return new List<PermissionDto>();
                }

                var result = permissions.Select(p => new PermissionDto
                {
                    Id = p["_id"].AsString,
                    Name = p["PermissionName"].AsString
                }).ToList();

                _logger.Info($"Consulta completada exitosamente. Total permisos: {result.Count}");
                return result;
            }
            catch (Exception ex)
            {
                _logger.Error("Error en GetAllPermissionsQueryHandler al obtener permisos.", ex);
                throw;
            }
        }
    }
}
