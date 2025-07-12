using MediatR;
using log4net;

using Usuarios.Application.DTOs;

using Usuarios.Infrastructure.Interfaces;

namespace Usuarios.Infrastructure.Queries.QueryHandlers
{
    public class GetAllRolesQueryHandler : IRequestHandler<GetAllRolesQuery, List<RoleDto>>
    {
        private readonly IRoleReadRepository _roleReadRepository;
        private static readonly ILog _logger = LogManager.GetLogger(typeof(GetAllRolesQueryHandler));

        public GetAllRolesQueryHandler(IRoleReadRepository roleReadRepository)
        {
            _roleReadRepository = roleReadRepository ?? throw new ArgumentNullException(nameof(roleReadRepository));
        }

        public async Task<List<RoleDto>> Handle(GetAllRolesQuery request, CancellationToken cancellationToken)
        {
            _logger.Info("Procesando consulta GetAllRolesQuery");

            try
            {
                var roles = await _roleReadRepository.GetAllAsync();

                if (roles == null || !roles.Any())
                {
                    _logger.Warn("No se encontraron roles en la base de datos.");
                    return new List<RoleDto>();
                }

                var result = roles.Select(r => new RoleDto
                {
                    Id = r.Id,
                    Name = r.Name,
                    Permissions = r.Permissions
                }).ToList();

                _logger.Info($"Consulta completada exitosamente. Total roles: {result.Count}");
                return result;
            }
            catch (Exception ex)
            {
                _logger.Error("Error en GetAllRolesQueryHandler al obtener roles.", ex);
                throw;
            }
        }
    }
}
