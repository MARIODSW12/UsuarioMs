using MediatR;

namespace Usuarios.Infrastructure.Queries
{
    public class GetPermissionsByRoleIdQuery : IRequest<List<string>?>
    {
        public string RoleId { get; }

        public GetPermissionsByRoleIdQuery(string roleId)
        {
            RoleId = roleId;
        }
    }
}
