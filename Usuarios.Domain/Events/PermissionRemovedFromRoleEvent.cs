using MediatR;

namespace Usuarios.Domain.Events
{
    public class PermissionRemovedFromRoleEvent : INotification
    {
        public string RoleId { get; }
        public string PermissionId { get; }
        public DateTime OccurredOn { get; }

        public PermissionRemovedFromRoleEvent(string roleId, string permissionId, DateTime occurredOn)
        {
            RoleId = roleId;
            PermissionId = permissionId;
            OccurredOn = occurredOn;
        }
    }
}
