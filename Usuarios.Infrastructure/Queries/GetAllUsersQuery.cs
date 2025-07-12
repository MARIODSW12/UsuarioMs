using MediatR;

using Usuarios.Application.DTOs;

namespace Usuarios.Infrastructure.Queries
{
    public class GetAllUsersQuery : IRequest<List<UserDto>>
    {
    }
}
