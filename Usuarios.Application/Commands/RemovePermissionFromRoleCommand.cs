using MediatR;

namespace Usuarios.Application.Commands
{
    public class RemovePermissionFromRoleCommand : IRequest<bool>
    {
        public string RoleId { get; }
        public string PermissionId { get; }

        public RemovePermissionFromRoleCommand(string roleId, string permissionId)
        {
            RoleId = roleId;
            PermissionId = permissionId;
        }
    }
}
