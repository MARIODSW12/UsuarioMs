using MediatR;

using Usuarios.Application.DTOs;

namespace Usuarios.Infrastructure.Queries
{
    public class GetAllRolesQuery : IRequest<List<RoleDto>>
    {
    }
}
