using MediatR;
using log4net;

using Usuarios.Application.DTOs;

using Usuarios.Infrastructure.Interfaces;

namespace Usuarios.Infrastructure.Queries.QueryHandlers
{
    public class GetAllUsersQueryHandler : IRequestHandler<GetAllUsersQuery, List<UserDto>>
    {
        private readonly IUserReadRepository _userReadRepository;
        private static readonly ILog _logger = LogManager.GetLogger(typeof(GetAllUsersQueryHandler));

        public GetAllUsersQueryHandler(IUserReadRepository userReadRepository)
        {
            _userReadRepository = userReadRepository ?? throw new ArgumentNullException(nameof(userReadRepository));
        }

        public async Task<List<UserDto>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
        {
            _logger.Info("Procesando consulta GetAllUsersQuery");

            try
            {
                var users = await _userReadRepository.GetAllAsync();

                if (users == null || !users.Any())
                {
                    _logger.Warn("No se encontraron usuarios en la base de datos.");
                    return new List<UserDto>();
                }

                var result = users.Select(u => new UserDto
                {
                    Id = u["_id"].AsString,
                    Name = u["name"].AsString,
                    LastName = u["lastName"].AsString,
                    Email = u["email"].AsString,
                    RoleId = u["roleId"].AsString
                }).ToList();

                _logger.Info($"Consulta completada exitosamente. Total usuarios: {result.Count}");
                return result;
            }
            catch (Exception ex)
            {
                _logger.Error("Error en GetAllUsersQueryHandler al obtener usuarios.", ex);
                throw;
            }
        }
    }
}
