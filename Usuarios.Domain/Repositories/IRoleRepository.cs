using Usuarios.Domain.Entities;
using Usuarios.Domain.ValueObjects;

namespace Usuarios.Domain.Repositories
{
    public interface IRoleRepository
    {
        Task<Role?> GetByIdAsync(string roleId); // Obtener un rol por su ID
        Task<bool> UpdateAsync(Role role); // Actualizar rol
        Task<bool> AddPermissionsAsync(VORoleId roleId, VORolePermissions permissionId); // Agregar permisos a un rol
    }
}
