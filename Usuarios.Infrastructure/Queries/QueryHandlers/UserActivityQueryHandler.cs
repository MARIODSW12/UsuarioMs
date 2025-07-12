using MediatR;
using log4net;

using Usuarios.Application.DTOs;

using Usuarios.Infrastructure.Interfaces;

namespace Usuarios.Infrastructure.Queries.QueryHandlers
{
    public class UserActivityQueryHandler : IRequestHandler<UserActivityQuery, IEnumerable<UserActivityDto>>
    {
        private readonly IUserActivityReadRepository _userActivityReadRepository;
        private static readonly ILog _logger = LogManager.GetLogger(typeof(UserActivityQueryHandler));

        public UserActivityQueryHandler(IUserActivityReadRepository userActivityReadRepository)
        {
            _userActivityReadRepository = userActivityReadRepository ?? throw new ArgumentNullException(nameof(userActivityReadRepository));
        }

        public async Task<IEnumerable<UserActivityDto>> Handle(UserActivityQuery request, CancellationToken cancellationToken)
        {
            _logger.Info($"Procesando consulta UserActivityQuery para el usuario {request.UserId}");

            try
            {
                var activities = await _userActivityReadRepository.GetByUserIdAsync(request.UserId, DateTime.Today);

                if (activities == null || !activities.Any())
                {
                    _logger.Warn($"No se encontraron actividades para el usuario {request.UserId}");
                    return new List<UserActivityDto>();
                }

                var result = activities.ConvertAll(activity => new UserActivityDto
                {
                    Id = activity["_id"].AsString,
                    UserId = activity["_userid"].AsString,
                    Action = activity["message"].AsString,
                    Timestamp = activity["timestamp"].AsLocalTime
                });

                _logger.Info($"Consulta completada exitosamente. Total actividades: {result.Count}");
                return result;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error en UserActivityQueryHandler al obtener actividades del usuario {request.UserId}", ex);
                throw;
            }
        }
    }
}
