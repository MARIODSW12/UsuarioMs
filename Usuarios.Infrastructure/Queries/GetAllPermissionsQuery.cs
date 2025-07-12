using MediatR;

using Usuarios.Application.DTOs;

namespace Usuarios.Infrastructure.Queries
{
    public class GetAllPermissionsQuery : IRequest<List<PermissionDto>>
    {
    }
}
