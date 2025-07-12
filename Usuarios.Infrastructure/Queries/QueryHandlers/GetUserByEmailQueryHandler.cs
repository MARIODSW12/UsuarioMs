using MediatR;
using log4net;

using Usuarios.Application.DTOs;

using Usuarios.Infrastructure.Interfaces;

namespace Usuarios.Infrastructure.Queries.QueryHandlers
{
    public class GetUserByEmailQueryHandler : IRequestHandler<GetUserByEmailQuery, UserByEmailDto>
    {
        private readonly IUserReadRepository _userReadRepository;
        private static readonly ILog _logger = LogManager.GetLogger(typeof(GetUserByEmailQueryHandler));

        public GetUserByEmailQueryHandler(IUserReadRepository userReadRepository)
        {
            _userReadRepository = userReadRepository ?? throw new ArgumentNullException(nameof(userReadRepository));
        }

        public async Task<UserByEmailDto> Handle(GetUserByEmailQuery request, CancellationToken cancellationToken)
        {
            _logger.Info($"Procesando consulta GetUserByEmailQuery para el email: {request.Email}");

            try
            {
                var user = await _userReadRepository.GetByEmailAsync(request.Email);

                if (user == null)
                {
                    _logger.Warn($"No se encontró usuario con el email: {request.Email}");
                    throw new KeyNotFoundException("Usuario no encontrado.");
                }

                var userDto = new UserByEmailDto
                {
                    userId = user.Id,
                    Name = user.Name,
                    LastName = user.LastName,
                    Email = user.Email,
                    RoleId = user.roleId,
                    Address = user.Address,
                    Phone = user.Phone
                };

                _logger.Info($"Usuario obtenido exitosamente con email: {request.Email}");
                return userDto;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error en GetUserByEmailQueryHandler al obtener usuario con email: {request.Email}", ex);
                throw;
            }
        }
    }
}
