using MediatR;

namespace Usuarios.Application.Commands
{

    public class AddPermissionToRoleCommand : IRequest<bool>
    {
        public string RoleId { get; }
        public string PermissionId { get; }

        public AddPermissionToRoleCommand(string roleId, string permissionId)
        {
            RoleId = roleId;
            PermissionId = permissionId;
        }
    }
}
