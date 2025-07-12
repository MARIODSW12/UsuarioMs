using MediatR;

namespace Usuarios.Domain.Events
{
    public class PermissionAddedToRoleEvent : INotification
    {
        public string RoleId { get; }
        public string PermissionId { get; }
        public DateTime OccurredOn { get; }

        public PermissionAddedToRoleEvent(string roleId, string permissionId, DateTime occurredOn)
        {
            RoleId = roleId;
            PermissionId = permissionId;
            OccurredOn = occurredOn;
        }
    }
}
