using MediatR;

using Usuarios.Application.DTOs;

namespace Usuarios.Infrastructure.Queries
{
    public class UserActivityQuery : IRequest<IEnumerable<UserActivityDto>>
    {
        public string? UserId { get; set; }
        public DateTime Timestamp { get; set; }

        public UserActivityQuery(
            string userId,
            DateTime timestamp
        )
        {
            UserId = userId;
            Timestamp = timestamp;
        }
    }
}
