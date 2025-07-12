using MongoDB.Bson;

using Usuarios.Application.DTOs;

using Usuarios.Domain.Entities;

namespace Usuarios.Infrastructure.Interfaces
{
    public interface IRoleReadRepository
    {
        Task<Role?> GetByIdAsync(string roleId); // Obtener un rol por su ID
        Task<List<RoleDto>> GetAllAsync();  // Obtener todos los roles
        Task<bool> AddPermissionsAsync(BsonDocument role); // Modificar permisos
        Task RemovePermissionByIdAsync(string roleId, string permissionId);
        Task<List<string>?> GetPermissionsByRoleIdAsync(string roleId);
    }
}
