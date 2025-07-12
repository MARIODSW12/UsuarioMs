using Usuarios.Domain.ValueObjects;

namespace Usuarios.Domain.Entities
{
    public class Role
    {
        public VORoleId Id { get; private set; }
        public VORoleName Name { get; private set; }
        public VORolePermissions PermissionIds { get; private set; }

        public Role(VORoleId id, VORoleName name, VORolePermissions permissionIds)
        {
            Id = id;
            Name = name;
            PermissionIds = permissionIds;
        }

        public void AddPermission(string permissionId)
        {
            if (!PermissionIds.HasPermission(permissionId))
            {
                PermissionIds.PermissionIds.Add(permissionId);
            }
        }

        public void RemovePermission(string permissionId)
        {
            PermissionIds.PermissionIds.Remove(permissionId);
        }
    }
}
